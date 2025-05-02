using e_learning.Data;
using e_learning.Models;
using e_learning.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace e_learning.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        public readonly AppDbContext _context;  // التأكد من أن اسم الـ DbContext هو AppDbContext وليس AppDbContextContext

        // البناء الصحيح مع AppDbContext
        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        // استرجاع الدورات
        public async Task<IEnumerable<Course>> GetCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        // استرجاع دورة باستخدام الـ ID
        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // إضافة دورة جديدة
        public async Task AddCourseAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        // تحديث دورة موجودة
        public async Task UpdateCourseAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        // حذف دورة باستخدام الـ ID
        public async Task DeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                                       .FirstOrDefaultAsync(c => c.Id == id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }
    }
}


