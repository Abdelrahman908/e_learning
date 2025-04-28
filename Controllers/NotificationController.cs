using e_learning.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ عرض كل الإشعارات للمستخدم الحالي
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // ✅ تحديد إشعار كمقروء
        [HttpPost("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound("❌ الإشعار غير موجود أو لا تملكه.");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok("✅ تم تحديد الإشعار كمقروء.");
        }

        // ✅ حذف إشعار
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound("❌ الإشعار غير موجود أو لا تملكه.");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok("✅ تم حذف الإشعار بنجاح.");
        }

        // 🧠 استخراج الـ UserId من التوكن
        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : null;
        }

        // ✅ حذف كل الإشعارات للمستخدم الحالي
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (!notifications.Any())
                return NotFound("🚫 لا توجد إشعارات لحذفها.");

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return Ok("✅ تم حذف كل الإشعارات الخاصة بك بنجاح.");
        }

    }
}
