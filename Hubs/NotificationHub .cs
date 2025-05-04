using e_learning.DTOs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace e_learning.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userId, out var intUserId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{intUserId}");
                    _logger.LogInformation($"User {intUserId} connected to NotificationHub");
                }
                else
                {
                    _logger.LogWarning("Invalid user ID format during connection");
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userId, out var intUserId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{intUserId}");
                    _logger.LogInformation($"User {intUserId} disconnected from NotificationHub");
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
                throw;
            }
        }

        // طريقة لإرسال إشعارات مباشرة لمستخدم معين
        public async Task SendNotificationToUser(int userId, NotificationDto notification)
        {
            await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }
    }
}