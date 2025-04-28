namespace e_learning.Models
{
    public class QuizResult
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }

}
