using System;

namespace e_learning.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }  // تغيير من Guid إلى int
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }   // حقل المستخدم
        public int? SenderId { get; set; }  // تغيير من string? إلى int? لتوحيد الأنواع
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string NotificationType { get; set; } = "General";
    }
}