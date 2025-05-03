namespace e_learning.DTOs
{
    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public decimal CompletionPercentage => TotalLessons > 0
            ? (decimal)CompletedLessons / TotalLessons * 100
            : 0;
        public DateTime? LastAccessed { get; set; }

    }
}