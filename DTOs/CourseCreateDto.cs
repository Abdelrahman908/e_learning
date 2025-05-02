using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace e_learning.DTOs.Courses
{
    public class CourseCreateDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
        public bool IsActive { get; set; }


        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int InstructorId { get; set; }

    }
}
