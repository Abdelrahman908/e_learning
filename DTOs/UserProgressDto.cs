namespace e_learning.DTOs
{
    public class UserProgressDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public double CompletionPercentage { get; set; }
        public double AverageQuizScore { get; set; }
        public List<LessonProgressDto> LessonsProgress { get; set; }
    }
}
