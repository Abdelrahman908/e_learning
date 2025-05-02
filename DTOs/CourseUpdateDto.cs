using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace e_learning.DTOs.Courses
{
    public class CourseUpdateDto
    {
        public int Id { get; set; } // Id ضروري ومطلوب
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
        public int CategoryId { get; set; }
        public int InstructorId { get; set; }
        public IFormFile? ImageFile { get; set; }


    }
}
