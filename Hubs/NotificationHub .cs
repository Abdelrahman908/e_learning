using Microsoft.AspNetCore.SignalR;

namespace e_learning.Hubs
{
    public class NotificationHub : Hub
    {
        // 🧠 عند الاتصال، اضف المستخدم لجروب خاص بيه
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnConnectedAsync();
        }

        // 🧠 عند قطع الاتصال، شيله من الجروب
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
