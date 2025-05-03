using e_learning.Enums.e_learning.Enums;

namespace e_learning.DTOs
{
    namespace e_learning.DTOs.Lessons
    {
        public class LessonResponseDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public LessonType Type { get; set; }
            public string Content { get; set; }
            public string VideoUrl { get; set; }
            public string PdfUrl { get; set; }
            public int Duration { get; set; }
            public bool IsFree { get; set; }
            public bool IsSequential { get; set; }
            public int Order { get; set; }
            public DateTime CreatedAt { get; set; }
            public int CourseId { get; set; }
            public string CourseTitle { get; set; }
            public QuizBriefDto Quiz { get; set; }
            public List<LessonMaterialDto> Materials { get; set; }
            public LessonProgressDto Progress { get; set; }
        }
    }
}
