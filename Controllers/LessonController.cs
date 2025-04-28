using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using e_learning.Data;
using e_learning.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LessonController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ جلب جميع الدروس
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetLessons()
        {
            return await _context.Lessons.Include(l => l.Course).ToListAsync();
        }

        // 🔐 فقط المدرس يضيف دروس
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<ActionResult<Lesson>> CreateLesson(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLessons), new { id = lesson.Id }, lesson);
        }

        // 📥 رفع ملف مرتبط بالدرس (فقط للمدرسين)
        [Authorize(Roles = "Instructor")]
        [HttpPost("{lessonId}/upload-material")]
        public async Task<IActionResult> UploadMaterial(int lessonId, IFormFile file)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
                return NotFound("❌ الدرس غير موجود.");

            if (file == null || file.Length == 0)
                return BadRequest("⚠️ الملف غير موجود أو فارغ.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            lesson.AttachmentUrl = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { message = "✅ تم رفع الملف بنجاح.", url = lesson.AttachmentUrl });
        }
    }
}
