using e_learning.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace e_learning.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        public int? SenderId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string NotificationType { get; set; } = "General";

        // Navigation properties
        public virtual User User { get; set; } = null!;     // المستلم (مطلوب)
        public virtual User? Sender { get; set; }           // المرسل (اختياري)
    }
}
