namespace e_learning.DTOs
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? Text { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsSeen { get; set; }
        public int? ReplyToMessageId { get; set; }
    }
}
