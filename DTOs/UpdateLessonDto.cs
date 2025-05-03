using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class UpdateLessonDto
    {
        [StringLength(200, ErrorMessage = "العنوان يجب أن لا يتجاوز 200 حرف")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "الوصف يجب أن لا يتجاوز 1000 حرف")]
        public string Description { get; set; }

        [Url(ErrorMessage = "رابط الفيديو غير صالح")]
        public string VideoUrl { get; set; }

        [FileExtensions(Extensions = "pdf", ErrorMessage = "يجب أن يكون الملف من نوع PDF")]
        public IFormFile PdfFile { get; set; }

        public string Content { get; set; }
        public int? Duration { get; set; }
        public bool? IsFree { get; set; }
        public bool? IsSequential { get; set; }
        public int? Order { get; set; }
    }
}
