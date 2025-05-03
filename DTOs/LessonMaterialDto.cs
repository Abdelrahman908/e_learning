namespace e_learning.DTOs
{
    public class LessonMaterialDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Description { get; set; }
        public long FileSize { get; set; }
        public string FileSizeFormatted { get; set; } // مثل "2.5 MB"
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } // اسم المستخدم
    }
}