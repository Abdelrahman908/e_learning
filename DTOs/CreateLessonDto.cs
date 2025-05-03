using System.ComponentModel.DataAnnotations;
using e_learning.Enums.e_learning.Enums;
using ExpressiveAnnotations.Attributes;

namespace e_learning.DTOs
{
    public class CreateLessonDto
    {
        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        [StringLength(200, ErrorMessage = "العنوان يجب أن لا يتجاوز 200 حرف")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "الوصف يجب أن لا يتجاوز 1000 حرف")]
        public string Description { get; set; }

        [Required(ErrorMessage = "نوع الدرس مطلوب")]
        public LessonType Type { get; set; }

        [Url(ErrorMessage = "رابط الفيديو غير صالح")]
        public string VideoUrl { get; set; }

        [FileExtensions(Extensions = "pdf", ErrorMessage = "يجب أن يكون الملف من نوع PDF")]
        public IFormFile PdfFile { get; set; }

        [RequiredIf(nameof(Type), LessonType.Text, ErrorMessage = "المحتوى النصي مطلوب")]
        public string Content { get; set; }

        [Range(1, 500, ErrorMessage = "المدة يجب أن تكون بين 1 و 500 دقيقة")]
        public int Duration { get; set; }

        public bool IsFree { get; set; } = false;
        public bool IsSequential { get; set; } = false;
        public int? Order { get; set; }
    }
}
