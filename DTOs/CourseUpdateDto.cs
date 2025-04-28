using Microsoft.AspNetCore.Http;

namespace e_learning.DTOs
{
    public class CourseUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public IFormFile? Image { get; set; }
        public bool? RemoveImage { get; set; } // ✅ لو المستخدم عايز يحذف الصورة
    }
}
