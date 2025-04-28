using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using e_learning.Data;
using e_learning.DTOs;
using e_learning.Hubs;
using e_learning.Models;

namespace e_learning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _env;

        public MessagesController(AppDbContext context, IHubContext<ChatHub> hubContext, IWebHostEnvironment env)
        {
            _context = context;
            _hubContext = hubContext;
            _env = env;
        }

        // 🛠️ إرسال رسالة نصية أو مرفق أو رد
        [HttpPost("send/{courseId}")]
        public async Task<IActionResult> SendMessage(int courseId, [FromForm] MessageCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var course = await _context.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound("❌ الكورس غير موجود.");

            var isInstructor = course.InstructorId == userId;
            var isStudentEnrolled = course.Enrollments.Any(e => e.UserId == userId);
            if (!isInstructor && !isStudentEnrolled)
                return Forbid("🚫 ليس لديك صلاحية.");

            string? attachmentUrl = null;
            if (dto.File != null)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                var fullPath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }
                attachmentUrl = $"/uploads/{fileName}";
            }

            var message = new Message
            {
                SenderId = userId,
                CourseId = courseId,
                Text = dto.Text ?? "",
                AttachmentUrl = attachmentUrl,
                IsSeen = false,
                ReplyToMessageId = dto.ReplyToMessageId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Course_{courseId}")
                .SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    SenderId = userId,
                    message.Text,
                    message.AttachmentUrl,
                    message.SentAt,
                    message.ReplyToMessageId
                });

            return Ok("✅ تم إرسال الرسالة.");
        }

        // 🛠️ عرض رسائل الكورس مع فلترة وباجيناشن
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetMessages(int courseId, int page = 1, int pageSize = 20, string? search = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var course = await _context.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound("❌ الكورس غير موجود.");

            var isInstructor = course.InstructorId == userId;
            var isStudentEnrolled = course.Enrollments.Any(e => e.UserId == userId);
            if (!isInstructor && !isStudentEnrolled)
                return Forbid("🚫 ليس لديك صلاحية.");

            var query = _context.Messages
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Sender)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => m.Text.Contains(search));

            if (fromDate.HasValue)
                query = query.Where(m => m.SentAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(m => m.SentAt <= toDate.Value);

            var totalMessages = await query.CountAsync();

            var messages = await query
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    SenderName = m.Sender.FullName,
                    m.Text,
                    m.AttachmentUrl,
                    m.SentAt,
                    m.IsSeen,
                    m.ReplyToMessageId
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalMessages,
                Messages = messages
            });
        }

        // 🛠️ عداد الرسائل الغير مقروءة
        [HttpGet("unseen-count/{courseId}")]
        public async Task<IActionResult> GetUnseenMessagesCount(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var count = await _context.Messages
                .Where(m => m.CourseId == courseId && m.SenderId != userId && !m.IsSeen)
                .CountAsync();

            return Ok(new { UnseenMessages = count });
        }

        // 🛠️ تعليم الرسائل كمقروءة
        [HttpPost("mark-seen/{courseId}")]
        public async Task<IActionResult> MarkMessagesAsSeen(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var unseenMessages = await _context.Messages
                .Where(m => m.CourseId == courseId && m.SenderId != userId && !m.IsSeen)
                .ToListAsync();

            if (unseenMessages.Any())
            {
                foreach (var msg in unseenMessages)
                {
                    msg.IsSeen = true;
                }
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"Course_{courseId}")
                    .SendAsync("MessagesSeen", new { UserId = userId });
            }

            return Ok("✅ تم تحديث حالة الرسائل.");
        }

        // 🛠️ كتابة Typing Indicator
        [HttpPost("typing/{courseId}")]
        public async Task<IActionResult> SendTyping(int courseId)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name)!;
            await _hubContext.Clients.Group($"Course_{courseId}")
                .SendAsync("Typing", new { UserName = userName });

            return Ok();
        }

        // 🛠️ تعديل رسالة
        [HttpPut("edit/{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] MessageEditDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return NotFound("❌ الرسالة غير موجودة.");
            if (message.SenderId != userId) return Forbid("🚫 لا يمكنك تعديل هذه الرسالة.");

            message.Text = dto.NewText;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Course_{message.CourseId}")
                .SendAsync("MessageEdited", new { MessageId = message.Id, NewText = message.Text });

            return Ok("✅ تم تعديل الرسالة.");
        }

        // 🛠️ حذف رسالة
        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return NotFound("❌ الرسالة غير موجودة.");
            if (message.SenderId != userId) return Forbid("🚫 لا يمكنك حذف هذه الرسالة.");

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Course_{message.CourseId}")
                .SendAsync("MessageDeleted", new { MessageId = message.Id });

            return Ok("✅ تم حذف الرسالة.");
        }

        // 🛠️ React Like/Heart على رسالة
        [HttpPost("react/{messageId}")]
        public async Task<IActionResult> ReactToMessage(int messageId, [FromBody] MessageReactionDto dto)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return NotFound("❌ الرسالة غير موجودة.");

            await _hubContext.Clients.Group($"Course_{message.CourseId}")
                .SendAsync("MessageReacted", new
                {
                    MessageId = message.Id,
                    Reaction = dto.ReactionType
                });

            return Ok("✅ تم إرسال الريأكشن.");
        }
    }
}
