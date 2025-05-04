using e_learning.Data;
using e_learning.DTOs;
using e_learning.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [Produces("application/json")]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationController> _logger;
        private readonly int _currentUserId;

        public NotificationController(
            AppDbContext context,
            ILogger<NotificationController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _currentUserId = int.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (_currentUserId == 0)
                {
                    _logger.LogWarning("Unauthorized attempt to access notifications");
                    return Unauthorized(new ApiResponse(false, "غير مصرح به"));
                }

                var query = _context.Notifications
                    .AsNoTracking()
                    .Where(n => n.UserId == _currentUserId)
                    .OrderByDescending(n => n.CreatedAt);

                var totalCount = await query.CountAsync();
                var notifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        UserId = n.UserId,
                        SenderId = n.SenderId,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt,
                        NotificationType = n.NotificationType,
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>(true, "تم جلب الإشعارات بنجاح", new
                {
                    Total = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Notifications = notifications
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for user {UserId}", _currentUserId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        [HttpPatch("{id:int}/read-status")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReadStatus(int id, [FromBody] bool isRead)
        {
            try
            {
                if (_currentUserId == 0)
                {
                    _logger.LogWarning("Unauthorized attempt to update notification status");
                    return Unauthorized(new ApiResponse(false, "غير مصرح به"));
                }

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == _currentUserId);

                if (notification == null)
                {
                    _logger.LogWarning("Notification not found - NotificationId: {NotificationId}, UserId: {UserId}", id, _currentUserId);
                    return NotFound(new ApiResponse(false, "الإشعار غير موجود أو لا تملكه"));
                }

                notification.IsRead = isRead;
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification {NotificationId} read status updated to {IsRead} by {UserId}", id, isRead, _currentUserId);
                return Ok(new ApiResponse(true, "تم تحديث حالة الإشعار بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification status for NotificationId: {NotificationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                if (_currentUserId == 0)
                {
                    _logger.LogWarning("Unauthorized attempt to delete notification");
                    return Unauthorized(new ApiResponse(false, "غير مصرح به"));
                }

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == _currentUserId);

                if (notification == null)
                {
                    _logger.LogWarning("Notification not found for deletion - NotificationId: {NotificationId}, UserId: {UserId}", id, _currentUserId);
                    return NotFound(new ApiResponse(false, "الإشعار غير موجود أو لا تملكه"));
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification deleted - NotificationId: {NotificationId} by UserId: {UserId}", id, _currentUserId);
                return Ok(new ApiResponse(true, "تم حذف الإشعار بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification - NotificationId: {NotificationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }
    }
}