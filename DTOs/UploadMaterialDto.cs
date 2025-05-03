using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class UploadMaterialDto
    {
        [Required(ErrorMessage = "الملف مطلوب")]
        public IFormFile File { get; set; }

        [StringLength(100, ErrorMessage = "الوصف يجب أن لا يتجاوز 100 حرف")]
        public string Description { get; set; }
    }
}
