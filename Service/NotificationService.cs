using e_learning.DTOs;
using e_learning.Data;
using Microsoft.EntityFrameworkCore;

namespace e_learning.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendNotificationAsync(NotificationDto notification)
        {
            // تنفيذ إرسال الإشعار
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            // تنفيذ جلب إشعارات المستخدم
            return new List<NotificationDto>();
        }
    }
}