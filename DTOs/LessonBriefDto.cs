namespace e_learning.DTOs
{
    public class LessonBriefDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public bool IsFree { get; set; }
    }
}