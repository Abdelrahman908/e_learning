using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.Models;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Instructor")]
    public class InstructorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InstructorController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ عرض كورسات المدرس
        [HttpGet("my-courses/{instructorId}")]
        public async Task<IActionResult> GetMyCourses(int instructorId)
        {
            var courses = await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.Lessons)
                .ToListAsync();

            return Ok(courses);
        }

        // ✅ عدد الطلاب في كورس معين
        [HttpGet("students-count/{courseId}")]
        public async Task<IActionResult> GetStudentCount(int courseId)
        {
            int count = await _context.Enrollments.CountAsync(e => e.CourseId == courseId);
            return Ok(new { courseId = courseId, studentCount = count });
        }

        // ✅ إضافة درس
        [HttpPost("add-lesson")]
        public async Task<IActionResult> AddLesson([FromBody] Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return Ok("تمت إضافة الدرس ✅");
        }
    }
}
