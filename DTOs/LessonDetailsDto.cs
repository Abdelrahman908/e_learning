using e_learning.Enums.e_learning.Enums;

namespace e_learning.DTOs
{
    public class LessonDetailsDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public LessonType Type { get; set; }
        public string VideoUrl { get; set; }
        public string PdfUrl { get; set; }
        public string Content { get; set; }
        public int Duration { get; set; }
        public bool IsFree { get; set; }
        public bool IsSequential { get; set; }
        public int Order { get; set; }
        public QuizDto Quiz { get; set; }
        public List<LessonMaterialDto> Materials { get; set; }
    }
}
