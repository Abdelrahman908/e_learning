using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace e_learning.DTOs.Courses
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

    namespace e_learning.DTOs.Courses
    {
        public class CourseCreateDto
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public decimal Price { get; set; }
            public bool IsActive { get; set; }
            public int CategoryId { get; set; } // تم تغييره من Guid إلى int
        }
    }
}
