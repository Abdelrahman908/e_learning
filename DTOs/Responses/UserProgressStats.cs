namespace e_learning.DTOs.Responses
{
    public class UserProgressStats
    {
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public double CompletionPercentage { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
        public double AverageQuizScore { get; set; }
        public int TotalQuizzesTaken { get; set; }
        public DateTime? LastQuizAttempt { get; set; }
        public List<LessonProgressDto> RecentProgress { get; set; } = new();
    
    }
}