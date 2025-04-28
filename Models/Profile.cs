// Models/Profile.cs
using System;

namespace e_learning.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ربط البروفايل بالمستخدم
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
