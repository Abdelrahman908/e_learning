﻿using Microsoft.AspNetCore.Mvc;
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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        // ✅ بعد التعديل: لدعم int? بدل int
        private async Task<bool> ValidateCategoryId(int? categoryId)
        {
            if (categoryId == null)
                return false;

            return await _context.Categories.AnyAsync(c => c.Id == categoryId.Value);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddCourse([FromForm] CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await ValidateCategoryId(dto.CategoryId))
                return BadRequest($"Category with ID {dto.CategoryId} does not exist.");

            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("User ID not found or invalid in token.");

            var course = new Course
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsActive = dto.IsActive,
                CategoryId = dto.CategoryId,
                InstructorId = userId,
                CreatedBy = userId
            };

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(dto.Image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid image format. Only .jpg, .jpeg, .png are allowed.");
                }

                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine("wwwroot/images/courses", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Image.CopyToAsync(stream);

                course.ImageUrl = $"/images/courses/{fileName}";
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course added successfully", course });
        }

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
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateDto model)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("Course not found.");

            // تعديل: استبدال HasValue بالتحقق null
            if (model.CategoryId != null && model.CategoryId != course.CategoryId && !await ValidateCategoryId(model.CategoryId))
                return BadRequest($"Category with ID {model.CategoryId} does not exist.");

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

            if (model.CategoryId != null)
                course.CategoryId = model.CategoryId.Value;

            if (model.InstructorId > 0)
                course.InstructorId = model.InstructorId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Course updated successfully." });
        }

        [HttpPut("update-with-image/{id}")]
        public async Task<IActionResult> UpdateCourseWithImage(int id, [FromForm] CourseUpdateDto model)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("Course not found.");

            if (model.CategoryId != null && model.CategoryId != course.CategoryId && !await ValidateCategoryId(model.CategoryId))
                return BadRequest($"Category with ID {model.CategoryId} does not exist.");

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

            if (model.CategoryId != null)
                course.CategoryId = model.CategoryId.Value;

            if (model.InstructorId > 0)
                course.InstructorId = model.InstructorId;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid image format. Only .jpg, .jpeg, .png are allowed.");
                }

                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine("wwwroot/images/courses", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                course.ImageUrl = $"/images/courses/{fileName}";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Course updated with image successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("Course not found.");

            var reviews = _context.Reviews.Where(r => r.CourseId == id);
            _context.Reviews.RemoveRange(reviews);

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
