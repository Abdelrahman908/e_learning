namespace e_learning.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? VideoUrl { get; set; }
        public string? AttachmentUrl { get; set; } // ✨ هنا الرابط بتاع الملف


        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
