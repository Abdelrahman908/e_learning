using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace e_learning.DTOs.Courses
{
    public class CourseCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }

        public IFormFile? Image { get; set; } // للرفع من الواجهة
    }
}
