namespace e_learning.DTOs
{
    public class LessonProgressDto
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; }
        public bool IsCompleted { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan TimeSpent { get; set; }
    }
}
