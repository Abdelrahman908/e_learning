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
        private readonly IGoogleDriveService _googleDriveService;

        public LessonService(AppDbContext context, IGoogleDriveService googleDriveService)
        {
            _context = context;
            _googleDriveService = googleDriveService;
        }

        public async Task<bool> LessonExists(int courseId, int lessonId)
        {
            return await _context.Lessons.AnyAsync(l => l.CourseId == courseId && l.Id == lessonId);
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
                    IsFree = l.IsFree,
                    VideoUrl = l.VideoUrl
                })
                .ToListAsync();

            return ApiResponse<List<LessonBriefDto>>.SuccessResponse(lessons);
        }

        public async Task<ApiResponse<LessonResponseDto>> CreateLesson(int courseId, CreateLessonDto dto, int userId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                return ApiResponse<LessonResponseDto>.NotFound("الكورس غير موجود");

            string videoUrl = null;
            string driveFileId = null;

            if (dto.VideoFile != null)
            {
                const string driveFolderId = "1JcUNRUFJoi2mOvClyPXw1OhmpqlO4WmK"; // ← Folder ID ثابت
                driveFileId = await _googleDriveService.UploadFileAsync(dto.VideoFile, driveFolderId);
                videoUrl = $"https://drive.google.com/file/d/{driveFileId}/view";
            }

            var lesson = new Lesson
            {
                Title = dto.Title,
                Description = dto.Description,
                Content = dto.Content,
                Type = dto.Type,
                Duration = dto.Duration,
                IsFree = dto.IsFree,
                IsSequential = dto.IsSequential,
                Order = dto.Order ?? 1,
                CourseId = courseId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                VideoUrl = videoUrl,
                DriveFileId = driveFileId
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            var response = new LessonResponseDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Content = lesson.Content,
                Type = lesson.Type,
                Duration = lesson.Duration,
                IsFree = lesson.IsFree,
                IsSequential = lesson.IsSequential,
                Order = lesson.Order,
                VideoUrl = lesson.VideoUrl,
                PdfUrl = null,
                CreatedAt = lesson.CreatedAt,
                CourseId = course.Id,
                CourseTitle = course.Title,
                Materials = new List<LessonMaterialDto>(),
                Quiz = null,
                Progress = null
            };

            return ApiResponse<LessonResponseDto>.SuccessResponse(response, "تم إنشاء الدرس بنجاح", 201);
        }

        public async Task<ApiResponse<LessonResponseDto>> GetLessonDetails(int courseId, int lessonId, ClaimsPrincipal user)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Quiz)
                .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Id == lessonId);

            if (lesson == null)
                return ApiResponse<LessonResponseDto>.NotFound("الدرس غير موجود");

            var materials = await _context.LessonMaterials
                .Where(m => m.LessonId == lessonId)
                .Select(m => new LessonMaterialDto
                {
                    Id = m.Id,
                    FileName = m.FileName,
                    FileUrl = m.FileUrl,
                    Description = m.Description
                })
                .ToListAsync();

            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var progress = await _context.LessonProgresses
                .Where(p => p.LessonId == lessonId && p.UserId == userId)
                .Select(p => new LessonProgressDto
                {
                    Id = p.Id,
                    IsCompleted = p.IsCompleted,
                    CompletedAt = p.CompletedAt
                })
                .FirstOrDefaultAsync();

            var response = new LessonResponseDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Content = lesson.Content,
                Type = lesson.Type,
                Duration = lesson.Duration,
                IsFree = lesson.IsFree,
                IsSequential = lesson.IsSequential,
                Order = lesson.Order,
                VideoUrl = lesson.VideoUrl,
                PdfUrl = null,
                CreatedAt = lesson.CreatedAt,
                CourseId = lesson.Course.Id,
                CourseTitle = lesson.Course.Title,
                Materials = materials,
                Quiz = lesson.Quiz != null ? new QuizBriefDto { Id = lesson.Quiz.Id, Title = lesson.Quiz.Title } : null,
                Progress = progress
            };

            return ApiResponse<LessonResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<LessonResponseDto>> UpdateLesson(int courseId, int lessonId, UpdateLessonDto dto)
        {
            var lesson = await _context.Lessons.Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Id == lessonId);

            if (lesson == null)
                return ApiResponse<LessonResponseDto>.NotFound("الدرس غير موجود");

            lesson.Title = dto.Title ?? lesson.Title;
            lesson.Description = dto.Description ?? lesson.Description;
            lesson.Content = dto.Content ?? lesson.Content;
            lesson.Duration = dto.Duration ?? lesson.Duration;
            lesson.IsFree = dto.IsFree ?? lesson.IsFree;
            lesson.UpdatedAt = DateTime.UtcNow;

            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();

            var response = new LessonResponseDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Content = lesson.Content,
                Type = lesson.Type,
                Duration = lesson.Duration,
                IsFree = lesson.IsFree,
                IsSequential = lesson.IsSequential,
                Order = lesson.Order,
                VideoUrl = lesson.VideoUrl,
                PdfUrl = null,
                CreatedAt = lesson.CreatedAt,
                CourseId = lesson.Course.Id,
                CourseTitle = lesson.Course.Title,
                Materials = new List<LessonMaterialDto>(),
                Quiz = null,
                Progress = null
            };

            return ApiResponse<LessonResponseDto>.SuccessResponse(response, "تم تحديث الدرس بنجاح");
        }

        public async Task<ApiResponse> DeleteLesson(int courseId, int lessonId)
        {
            var lesson = await _context.Lessons.Include(l => l.Materials)
                .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Id == lessonId);

            if (lesson == null)
                return ApiResponse.NotFound("الدرس غير موجود");

            foreach (var material in lesson.Materials)
            {
                if (!string.IsNullOrEmpty(material.DriveFileId))
                {
                    await _googleDriveService.DeleteFileAsync(material.DriveFileId);
                }
            }

            if (!string.IsNullOrEmpty(lesson.DriveFileId))
            {
                await _googleDriveService.DeleteFileAsync(lesson.DriveFileId);
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return ApiResponse.Ok("تم حذف الدرس بنجاح");
        }
    }
}
