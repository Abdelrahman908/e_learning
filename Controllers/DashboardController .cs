using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.DTOs;
using System.Security.Claims;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // 🧑‍🎓 Dashboard للطالب
        [Authorize(Roles = "Student")]
        [HttpGet("student")]
        public async Task<IActionResult> GetStudentDashboard()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("⚠️ لم يتم التحقق من هوية المستخدم.");

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .ToListAsync();

            var totalPayments = await _context.Payments
                .Where(p => p.UserId == userId)
                .SumAsync(p => p.Amount);

            var dto = new StudentDashboardDto
            {
                EnrolledCourses = enrollments.Count,
                TotalPaid = totalPayments
            };

            return Ok(dto);
        }

        // 🧑‍🏫 Dashboard للمدرس
        [Authorize(Roles = "Instructor")]
        [HttpGet("instructor")]
        public async Task<IActionResult> GetInstructorDashboard()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("⚠️ لم يتم التحقق من هوية المستخدم.");

            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Include(c => c.Enrollments)
                .Include(c => c.Payments)
                .ToListAsync();

            var totalEarnings = courses
                .SelectMany(c => c.Payments!)
                .Sum(p => p.Amount);

            var dto = new InstructorDashboardDto
            {
                CoursesCount = courses.Count,
                TotalStudents = courses.Sum(c => c.Enrollments?.Count ?? 0),
                TotalEarnings = totalEarnings
            };

            return Ok(dto);
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalStudents = await _context.Users.CountAsync(u => u.Role == "Student");
            var totalInstructors = await _context.Users.CountAsync(u => u.Role == "Instructor");
            var totalCourses = await _context.Courses.CountAsync();
            var activeCourses = await _context.Courses.CountAsync(c => c.IsActive == true);
            var totalRevenue = await _context.Payments.SumAsync(p => p.Amount);
            var totalEnrollments = await _context.Enrollments.CountAsync();

            var topByRevenue = await _context.Courses
                .Include(c => c.Payments)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .OrderByDescending(c => c.Payments.Sum(p => p.Amount))
                .Take(5)
                .Select(c => new CourseStatsDto
                {
                    CourseId = c.Id,
                    Title = c.Title ?? "غير محدد",
                    Enrollments = c.Enrollments.Count,
                    TotalRevenue = c.Payments.Sum(p => p.Amount),
                    AverageRating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0
                })
                .ToListAsync();

            var topByRating = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.Payments)
                .Include(c => c.Reviews)
                .Where(c => c.Reviews.Any())
                .OrderByDescending(c => c.Reviews.Average(r => r.Rating))
                .Take(5)
                .Select(c => new CourseStatsDto
                {
                    CourseId = c.Id,
                    Title = c.Title ?? "غير محدد",
                    Enrollments = c.Enrollments.Count,
                    TotalRevenue = c.Payments.Sum(p => p.Amount),
                    AverageRating = c.Reviews.Average(r => r.Rating)
                })
                .ToListAsync();

            var dto = new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCourses,
                ActiveCourses = activeCourses,
                TotalRevenue = totalRevenue,
                TotalEnrollments = totalEnrollments,
                TopCoursesByRevenue = topByRevenue,
                TopCoursesByRating = topByRating
            };

            return Ok(dto);
        }


    }
}