using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Models;
using e_learning.Data;
using e_learning.DTOs;
using System.Security.Claims;
using System.IO;
using e_learning.DTOs.e_learning.DTOs;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                .AsNoTracking()
                .Select(c => new CourseResponseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    InstructorName = c.Instructor != null ? c.Instructor.FullName : "غير معروف",
                    AverageRating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : (double?)null,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Include(c => c.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound("❌ الكورس غير موجود.");

            var dto = new CourseResponseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                IsActive = course.IsActive,
                CreatedAt = course.CreatedAt,
                InstructorName = course.Instructor?.FullName ?? "غير معروف",
                AverageRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : (double?)null,
                ImageUrl = course.ImageUrl
            };

            return Ok(dto);
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoursesFiltered([FromQuery] CourseFilterDto filter)
        {
            var query = _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                .AsNoTracking()
                .AsQueryable();

            if (filter.MinPrice.HasValue)
                query = query.Where(c => c.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(c => c.Price <= filter.MaxPrice.Value);

            if (filter.MinRating.HasValue)
                query = query.Where(c =>
                    c.Reviews.Any() &&
                    c.Reviews.Average(r => r.Rating) >= filter.MinRating.Value);

            var totalCourses = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCourses / (double)filter.PageSize);

            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CourseResponseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    CreatedAt = c.CreatedAt,
                    InstructorName = c.Instructor.FullName,
                    AverageRating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();

            return Ok(new
            {
                currentPage = filter.Page,
                totalPages,
                totalCourses,
                results = courses
            });
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ToggleCourseActivation(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("❌ الكورس غير موجود.");

            course.IsActive = !course.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { status = course.IsActive ? "✅ مفعل" : "❌ غير مفعل" });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<ActionResult<CourseResponseDto>> CreateCourse([FromForm] CourseCreateDto dto)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var course = new Course
            {
                Title = dto.Title!,
                Description = dto.Description,
                Price = dto.Price,
                InstructorId = instructorId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Courses", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await dto.Image.CopyToAsync(stream);

                course.ImageUrl = "/images/" + fileName;
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var response = new CourseResponseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                InstructorName = User.Identity?.Name,
                IsActive = course.IsActive,
                CreatedAt = course.CreatedAt,
                ImageUrl = course.ImageUrl
            };

            return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, response);
        }

        [HttpGet("home")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHomeCourses([FromQuery] string? search)
        {
            var query = _context.Courses
                .Include(c => c.Instructor)
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.Title, $"%{search}%") ||
                    (c.Description != null && EF.Functions.Like(c.Description, $"%{search}%")));
            }

            var result = await query
                .Take(10)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    Description = c.Description != null ? c.Description.Substring(0, Math.Min(100, c.Description.Length)) + "..." : "",
                    InstructorName = c.Instructor != null ? c.Instructor.FullName : "غير معروف",
                    c.ImageUrl
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
