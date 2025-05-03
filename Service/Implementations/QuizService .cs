using AutoMapper;
using e_learning.Data;
using e_learning.DTOs;
using e_learning.Enums;
using e_learning.Models;
using e_learning.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace e_learning.Service.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuizService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<QuizDto> CreateQuiz(CreateQuizDto dto, int userId)
        {
            var quiz = _mapper.Map<Quiz>(dto);
            quiz.CreatedAt = DateTime.UtcNow;
            quiz.CreatedBy = userId;

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return _mapper.Map<QuizDto>(quiz);
        }

        public async Task<QuizDetailsDto> GetQuizDetails(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            return _mapper.Map<QuizDetailsDto>(quiz);
        }

        public async Task<QuizDetailsDto> GetQuizForStudent(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            var quizDetails = _mapper.Map<QuizDetailsDto>(quiz);

            foreach (var question in quizDetails.Questions)
            {
                foreach (var choice in question.Choices)
                {
                    choice.IsCorrect = false;
                }
            }

            return quizDetails;
        }

        public async Task<Quiz> GetQuizWithQuestionsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task<QuizResult> StartQuiz(int userId, int quizId)
        {
            if (!await CanUserTakeQuiz(userId, quizId))
                throw new InvalidOperationException("User cannot take this quiz at this time");

            var quiz = await GetQuizWithQuestionsAsync(quizId);
            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            var quizResult = new QuizResult
            {
                UserId = userId,
                QuizId = quizId,
                StartedAt = DateTime.UtcNow,
                Status = QuizStatus.InProgress
            };

            _context.QuizResults.Add(quizResult);
            await _context.SaveChangesAsync();

            return quizResult;
        }

        public async Task<QuizResult> SubmitQuiz(int userId, int quizId, Dictionary<int, string> answers)
        {
            var quiz = await GetQuizWithQuestionsAsync(quizId);
            if (quiz == null)
                throw new KeyNotFoundException("Quiz not found");

            var quizResult = await _context.QuizResults
                .FirstOrDefaultAsync(r => r.UserId == userId &&
                                        r.QuizId == quizId &&
                                        r.Status == QuizStatus.InProgress);

            if (quizResult == null)
                throw new InvalidOperationException("No active quiz attempt found");

            int correctAnswers = 0;
            foreach (var question in quiz.Questions)
            {
                if (answers.TryGetValue(question.Id, out var userAnswer) &&
                    question.Answers.Any(a => a.Id.ToString() == userAnswer && a.IsCorrect))
                {
                    correctAnswers++;
                }
            }

            quizResult.CompletedAt = DateTime.UtcNow;
            quizResult.Score = (int)((double)correctAnswers / quiz.Questions.Count * 100);
            quizResult.IsPassed = quizResult.Score >= quiz.PassingScore;
            quizResult.Status = QuizStatus.Completed;

            await _context.SaveChangesAsync();
            return quizResult;
        }

        public async Task<QuizResultDto> GetQuizResult(int resultId)
        {
            var result = await _context.QuizResults
                .Include(r => r.Quiz)
                .Include(r => r.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                .Include(r => r.UserAnswers)
                    .ThenInclude(ua => ua.Answer)
                .FirstOrDefaultAsync(r => r.Id == resultId);

            if (result == null)
                throw new KeyNotFoundException("Quiz result not found");

            return _mapper.Map<QuizResultDto>(result);
        }

        public async Task<QuizStatsDto> GetQuizStatistics(int quizId)
        {
            var stats = new QuizStatsDto();
            var results = await _context.QuizResults
                .Where(r => r.QuizId == quizId && r.CompletedAt != null)
                .ToListAsync();

            stats.TotalAttempts = results.Count;
            stats.AverageScore = results.Any() ? results.Average(r => r.Score) : 0;
            stats.PassedCount = results.Count(r => r.IsPassed);
            stats.FailedCount = results.Count(r => !r.IsPassed);

            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Answers)
                .ToListAsync();

            stats.QuestionsStats = new List<QuestionStatsDto>();
            foreach (var question in questions)
            {
                var correctAnswers = await _context.UserAnswers
                    .CountAsync(ua => ua.QuestionId == question.Id &&
                                    ua.Answer.IsCorrect);

                var totalAnswers = await _context.UserAnswers
                    .CountAsync(ua => ua.QuestionId == question.Id);

                stats.QuestionsStats.Add(new QuestionStatsDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    CorrectPercentage = totalAnswers > 0 ?
                        (double)correctAnswers / totalAnswers * 100 : 0
                });
            }

            return stats;
        }

        public async Task<bool> CanUserTakeQuiz(int userId, int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return false;

            var hasCompletedLesson = await _context.LessonProgresses
                .AnyAsync(lp => lp.UserId == userId &&
                               lp.LessonId == quiz.LessonId &&
                               lp.IsCompleted);

            if (!hasCompletedLesson) return false;

            var attempts = await _context.QuizResults
                .CountAsync(r => r.UserId == userId &&
                               r.QuizId == quizId);

            return attempts < quiz.MaxAttempts;
        }

        public async Task<bool> DeleteQuiz(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return false;

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}