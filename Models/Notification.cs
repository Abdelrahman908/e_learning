﻿namespace e_learning.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? Title { get; set; } // ✅ ضيف دي 👈
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // الربط مع المستخدم

        public User User { get; set; }
    }
}
