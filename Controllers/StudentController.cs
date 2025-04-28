using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.Models;
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

        // ✅ عرض كل الكورسات المتاحة
        [HttpGet("all-courses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();

            return Ok(courses);
        }

        // ✅ تسجيل الطالب في كورس
        [HttpPost("enroll/{courseId}")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("⚠️ لم يتم التحقق من هوية المستخدم.");

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound("❌ الكورس غير موجود.");

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (alreadyEnrolled)
                return BadRequest("⚠️ أنت بالفعل مشترك في هذا الكورس.");

            var enrollment = new Enrollment
            {
                UserId = userId.Value,
                CourseId = courseId
            };

            _context.Enrollments.Add(enrollment);

            // 🔔 إشعار للمدرّس
            if (course.InstructorId != userId)
            {
                var student = await _context.Users.FindAsync(userId);
                if (student != null)
                {
                    var notification = new Notification
                    {
                        Title = "📥 انضمام جديد",
                        Message = $"👤 {student.FullName} انضم إلى كورسك: {course.Title}",
                        UserId = course.InstructorId
                    };
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();
            return Ok("✅ تم التسجيل بنجاح.");
        }

        // ✅ عرض كورسات الطالب
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("⚠️ لم يتم التحقق من هوية المستخدم.");

            var courseIds = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var courses = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Include(c => c.Lessons)
                .Include(c => c.Instructor)
                .ToListAsync();

            return Ok(courses);
        }

        // 🧠 استخراج UserId من التوكن
        private int? GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : (int?)null;
        }
    }
}
