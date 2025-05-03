using e_learning.Data;
using e_learning.DTOs;
using e_learning.DTOs.Responses;
using e_learning.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace e_learning.Service.Implementations
{
    public class ProgressService : IProgressService
    {
        private readonly AppDbContext _context;

        public ProgressService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProgressStats> GetUserProgress(int userId)
        {
            var stats = new UserProgressStats();

            var lessonProgress = await _context.LessonProgresses
                .Where(p => p.UserId == userId)
                .Include(p => p.Lesson)
                .ToListAsync();

            stats.TotalLessons = await _context.Lessons.CountAsync();
            stats.CompletedLessons = lessonProgress.Count(p => p.IsCompleted);
            stats.CompletionPercentage = stats.TotalLessons > 0 ?
                (double)stats.CompletedLessons / stats.TotalLessons * 100 : 0;
            stats.TotalTimeSpent = new TimeSpan(lessonProgress.Sum(p => p.TimeSpent.Ticks));

            var quizResults = await _context.QuizResults
                .Where(r => r.UserId == userId)
                .ToListAsync();

            stats.TotalQuizzesTaken = quizResults.Count;
            stats.AverageQuizScore = quizResults.Any() ?
                quizResults.Average(r => r.Score) : 0;
            stats.LastQuizAttempt = quizResults.Max(r => (DateTime?)r.CompletedAt);

            return stats;
        }

        public async Task<CourseProgressDto> GetCourseProgress(int courseId, int userId)
        {
            var progress = await _context.LessonProgresses
                .Where(p => p.Lesson.CourseId == courseId && p.UserId == userId)
                .ToListAsync();

            var totalLessons = await _context.Lessons
                .CountAsync(l => l.CourseId == courseId);

            var completedLessons = progress.Count(p => p.IsCompleted);

            return new CourseProgressDto
            {
                CourseId = courseId,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons
            };
        }

        public async Task<LessonProgressDto> GetLessonProgress(int lessonId, int userId)
        {
            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.LessonId == lessonId && p.UserId == userId);

            return new LessonProgressDto
            {
                LessonId = lessonId,
                IsCompleted = progress?.IsCompleted ?? false,
                ProgressPercentage = progress?.ProgressPercentage ?? 0
            };
        }
    }
}
