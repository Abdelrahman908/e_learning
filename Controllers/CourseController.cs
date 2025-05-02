using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.Models;
using e_learning.DTOs.Courses;
using e_learning.Repositories.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using e_learning.DTOs;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICourseRepository _courseRepository;

        public CourseController(AppDbContext context, ICourseRepository courseRepository)
        {
            _context = context;
            _courseRepository = courseRepository;
        }

        // POST: api/Course
        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = new Course
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsActive = dto.IsActive,
                CategoryId = dto.CategoryId,
                InstructorId = dto.InstructorId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(course);
        }

        // GET: api/Course/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound("Course not found.");

            double? avgRating = course.Reviews?.Any() == true
                ? course.Reviews.Average(r => r.Rating)
                : null;

            var courseDto = new CourseResponseDto
            {
                Id = course.Id,
                Name = course.Name,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                IsActive = course.IsActive,
                CategoryId = course.CategoryId,
                InstructorId = course.InstructorId,
                InstructorName = course.Instructor?.FullName,
                CategoryName = course.Category?.Name,
                AverageRating = avgRating,
                ImageUrl = course.ImageUrl
            };

            return Ok(courseDto);
        }

        // GET: api/Course
        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            try
            {
                var courses = await _courseRepository.GetCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Course/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseResponseDto courseRequest)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("Course not found.");

            course.Name = courseRequest.Name;
            course.Title = courseRequest.Title;
            course.Description = courseRequest.Description;
            course.Price = courseRequest.Price;
            course.IsActive = courseRequest.IsActive;
            course.CategoryId = courseRequest.CategoryId;
            course.InstructorId = courseRequest.InstructorId;

            await _context.SaveChangesAsync();

            return Ok("Course updated successfully.");
        }

        // PUT: api/Course/update-with-image
        [HttpPut("update-with-image")]
        public async Task<IActionResult> UpdateCourseWithImage([FromForm] CourseUpdateDto model)
        {
            var course = await _context.Courses.FindAsync(model.Id);
            if (course == null)
                return NotFound("Course not found.");

            // التحقق من الحقول المرسلة وتحديثها فقط إذا كانت غير فارغة أو null
            if (!string.IsNullOrWhiteSpace(model.Name))
                course.Name = model.Name;

            if (!string.IsNullOrWhiteSpace(model.Title))
                course.Title = model.Title;

            if (!string.IsNullOrWhiteSpace(model.Description))
                course.Description = model.Description;

            if (model.Price.HasValue)
                course.Price = model.Price.Value;

            if (model.IsActive.HasValue)
                course.IsActive = model.IsActive.Value;

            if (model.CategoryId > 0) // إذا كانت قيمة CategoryId صالحة
                course.CategoryId = model.CategoryId;

            if (model.InstructorId > 0) // إذا كانت قيمة InstructorId صالحة
                course.InstructorId = model.InstructorId;

            // إذا تم إرسال صورة جديدة، يتم تحديث الصورة
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine("wwwroot/images/courses", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                course.ImageUrl = $"/images/courses/{fileName}";
            }

            // حفظ التغييرات في قاعدة البيانات
            await _context.SaveChangesAsync();

            return Ok("Course updated with image successfully.");
        }

        // DELETE: api/Course/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("Course not found.");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
