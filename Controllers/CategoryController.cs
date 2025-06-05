using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.DTOs;
using e_learning.Models;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ إرجاع كل الكاتيجوريز فقط (بدون كورسات)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }

        // ✅ إرجاع كاتيجوري واحدة فقط
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound($"Category with ID {id} not found.");

            return Ok(category);
        }

        // ✅ إرجاع الكاتيجوريز مع الكورسات الخاصة بها (ملخص)
        [HttpGet("with-courses")]
        public async Task<ActionResult<IEnumerable<CategoryWithCoursesDto>>> GetWithCourses()
        {
            var categories = await _context.Categories
                .Include(c => c.Courses)
                .Select(c => new CategoryWithCoursesDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Courses = c.Courses.Select(course => new CourseSimpleDto
                    {
                        Id = course.Id,
                        Name = course.Name,
                        Title = course.Title,
                        Price = course.Price,
                    }).ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }

        // ✅ إضافة كاتيجوري جديدة
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Name = dto.Name!
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            });
        }
    }
}
