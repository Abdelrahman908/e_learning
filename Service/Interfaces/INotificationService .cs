using e_learning.DTOs;

namespace e_learning.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationDto notification);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
    }
}