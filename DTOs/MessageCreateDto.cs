namespace e_learning.DTOs
{
    public class MessageCreateDto
    {
        public string? Text { get; set; }
        public IFormFile? File { get; set; }
        public int? ReplyToMessageId { get; set; }
    }
}
