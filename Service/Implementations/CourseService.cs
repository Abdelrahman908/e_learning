﻿using Microsoft.EntityFrameworkCore;
using e_learning.DTOs.Courses;
using e_learning.Models;
using e_learning.Data;
using e_learning.Service.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace e_learning.Service.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;

        public CourseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseDetailsDto>> GetCoursesAsync(CourseFilterDto filter)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(filter.SearchTerm));
            }

            var courses = await query
                .AsNoTracking()  // لتقليل الحمل على الذاكرة في حالة الاستعلامات فقط
                .ToListAsync();

            return courses.Select(c => new CourseDetailsDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CategoryId = c.CategoryId,
                CategoryName = c.Category.Name,
            });
        }

        public async Task<CourseDetailsDto?> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)  // إذا كنت بحاجة إلى جلب الـ Category هنا
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return null;

            return new CourseDetailsDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                CategoryId = course.CategoryId,
                CategoryName = course.Category?.Name, // تأكد من وجود Category
            };
        }

        public async Task CreateCourseAsync(CourseCreateDto courseDto)
        {
            var course = new Course
            {
                Name = courseDto.Name,
                Description = courseDto.Description,
                CategoryId = courseDto.CategoryId,
                CreatedAt = DateTime.UtcNow // تأكد من إضافة الحقول مثل CreatedAt إذا كانت مهمة
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCourseAsync(int id, CourseUpdateDto courseDto)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
                throw new KeyNotFoundException("Course not found");

            course.Name = courseDto.Name ?? course.Name;
            course.Description = courseDto.Description ?? course.Description;
            course.CategoryId = courseDto.CategoryId;
            course.InstructorId = courseDto.InstructorId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
                throw new KeyNotFoundException("Course not found");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}
