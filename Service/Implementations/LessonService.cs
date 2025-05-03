using e_learning.Data;
using e_learning.DTOs;
using e_learning.DTOs.e_learning.DTOs.Lessons;
using e_learning.DTOs.Responses;
using e_learning.Models;
using e_learning.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning.Service.Implementations
{
    public class LessonService : ILessonService
    {
        private readonly AppDbContext _context;

        public LessonService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<LessonBriefDto>>> GetCourseLessons(int courseId, ClaimsPrincipal user)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => new LessonBriefDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    Order = l.Order,
                    IsFree = l.IsFree
                })
                .ToListAsync();

            return ApiResponse<List<LessonBriefDto>>.SuccessResponse(lessons);
        }

        public async Task<ApiResponse<LessonResponseDto>> CreateLesson(int courseId, CreateLessonDto dto, int userId)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists)
                return ApiResponse<LessonResponseDto>.NotFound("الكورس غير موجود");

            var lesson = new Lesson
            {
                Title = dto.Title,
                Description = dto.Description,
                CourseId = courseId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            var response = new LessonResponseDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description
            };

            return ApiResponse<LessonResponseDto>.SuccessResponse(response, "تم إنشاء الدرس بنجاح");
        }

        public async Task<ApiResponse<LessonResponseDto>> GetLessonDetails(int courseId, int lessonId, ClaimsPrincipal user)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Id == lessonId);

            if (lesson == null)
                return ApiResponse<LessonResponseDto>.NotFound("الدرس غير موجود");

            var response = new LessonResponseDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description
            };

            return ApiResponse<LessonResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<LessonResponseDto>> UpdateLesson(int courseId, int lessonId, UpdateLessonDto dto)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Id == lessonId);

            if (lesson == null)
                return ApiResponse<LessonResponseDto>.NotFound("الدرس غير موجود");

            lesson.Title = dto.Title ?? lesson.Title;
            lesson.Description = dto.Description ?? lesson.Description;
            lesson.UpdatedAt = DateTime.UtcNow;

            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();

            var response = new LessonResponseDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description
            };

            return ApiResponse<LessonResponseDto>.SuccessResponse(response, "تم تحديث الدرس بنجاح");
        }

        public async Task<ApiResponse> DeleteLesson(int courseId, int lessonId)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Id == lessonId);

            if (lesson == null)
                return ApiResponse.NotFound("الدرس غير موجود");

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return ApiResponse.Ok("تم حذف الدرس بنجاح");
        }
    }
}
