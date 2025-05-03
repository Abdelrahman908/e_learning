namespace e_learning.DTOs
{
    public class QuizStatsDto
    {
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public List<QuestionStatsDto> QuestionsStats { get; set; }
    }
}
