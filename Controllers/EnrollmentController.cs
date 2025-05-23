﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.DTOs.Responses;
using e_learning.Models;
using System.Security.Claims;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>تسجيل الطالب في كورس</summary>
        [HttpPost("{courseId}")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse(false, "⚠️ لم يتم التحقق من هوية المستخدم."));

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound(new ApiResponse(false, "❌ الكورس غير موجود."));

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (alreadyEnrolled)
                return BadRequest(new ApiResponse(false, "⚠️ أنت مسجل بالفعل في هذا الكورس."));

            var enrollment = new Enrollment
            {
                UserId = userId.Value,
                CourseId = courseId
            };

            _context.Enrollments.Add(enrollment);

            // إضافة إشعار للمُدرس عند انضمام طالب جديد
            if (course.InstructorId != userId)
            {
                var student = await _context.Users.FindAsync(userId);
                if (student != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = "📥 انضمام جديد",
                        Message = $"👤 {student.FullName} انضم إلى كورسك: {course.Title}",
                        UserId = course.InstructorId // تم تعديلها لتكون int بدلًا من ToString()
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse(true, "✅ تم التسجيل بنجاح"));
        }

        /// <summary>عرض كورسات الطالب</summary>
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse(false, "⚠️ لم يتم التحقق من هوية المستخدم."));

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, "✅ تم جلب الكورسات بنجاح", enrollments));
        }

        private int? GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : (int?)null;
        }
    }
}
