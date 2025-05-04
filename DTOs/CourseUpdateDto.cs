using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace e_learning.DTOs.Courses
{
    public class CourseUpdateDto
    {
        [Required]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal? Price { get; set; }

        public bool? IsActive { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be valid.")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "InstructorId must be valid.")]
        public int InstructorId { get; set; }

        public IFormFile? ImageFile { get; set; }
    }

}
