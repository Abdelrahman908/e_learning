namespace e_learning.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace e_learning.Models
    {
        public class LessonMaterial
        {
            public int Id { get; set; }

            [Required]
            public string FileName { get; set; }

            [Required]
            public string FileUrl { get; set; }

            public string Description { get; set; }

            public long FileSize { get; set; } // حجم الملف بالبايت

            [Required]
            public DateTime UploadedAt { get; set; }

            [Required]
            public string UploadedById { get; set; } // ID المستخدم الذي رفع الملف

            // العلاقات
            [Required]
            public int LessonId { get; set; }
            public Lesson Lesson { get; set; }
        }
    }
}
