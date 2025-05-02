using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.Models;
using e_learning.DTOs.Responses;
using System.Security.Claims;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>عرض جميع الكورسات</summary>
        [HttpGet("all-courses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<Course>>(true, "تم جلب الكورسات بنجاح", courses));
        }

        /// <summary>تسجيل الطالب في كورس</summary>
        [HttpPost("enroll/{courseId}")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse(false, "لم يتم التحقق من هوية المستخدم"));

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound(new ApiResponse(false, "الكورس غير موجود"));

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (alreadyEnrolled)
                return BadRequest(new ApiResponse(false, "أنت بالفعل مشترك في هذا الكورس"));

            _context.Enrollments.Add(new Enrollment
            {
                UserId = userId.Value,
                CourseId = courseId
            });

            if (course.InstructorId != userId)
            {
                var student = await _context.Users.FindAsync(userId);
                if (student != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "📥 انضمام جديد",
                        Message = $"👤 {student.FullName} انضم إلى كورسك: {course.Title}",
                        UserId = course.InstructorId // تم تعديلها من .ToString() إلى int
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse(true, "تم التسجيل في الكورس بنجاح"));
        }

        /// <summary>عرض كورسات الطالب</summary>
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse(false, "لم يتم التحقق من هوية المستخدم"));

            var courseIds = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var courses = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Include(c => c.Lessons)
                .Include(c => c.Instructor)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<Course>>(true, "تم جلب الكورسات الخاصة بك", courses));
        }

        private int? GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : null;
        }
    }
}
