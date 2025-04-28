namespace e_learning.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string? Text { get; set; } = string.Empty; // بقت Nullable عشان لو مفيش نص

        public string? AttachmentUrl { get; set; } // 📎 رابط الملف اللي اترفع (صورة/بي دي اف/الخ..)

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int? ReplyToMessageId { get; set; }
        public Message? ReplyToMessage { get; set; }



        public bool IsSeen { get; set; } = false;

    }
}
