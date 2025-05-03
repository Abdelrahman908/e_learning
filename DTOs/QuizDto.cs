namespace e_learning.DTOs
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } // بالدقائق
        public DateTime CreatedAt { get; set; }
    }
}