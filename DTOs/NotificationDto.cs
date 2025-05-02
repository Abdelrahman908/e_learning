using System;

namespace e_learning.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
       public int UserId { get; set; }   // حقل المستخدم
        public string? SenderId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } // تمت إضافتها
        public string NotificationType { get; set; } = "General";
    }
}