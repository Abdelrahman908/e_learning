using e_learning.DTOs;
using e_learning.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace e_learning.Service.Interfaces
{
    public interface IQuizService
    {
        // الوظائف الأساسية
        Task<Quiz> GetQuizWithQuestionsAsync(int quizId);
        Task<QuizResult> StartQuiz(int userId, int quizId);
        Task<QuizResult> SubmitQuiz(int userId, int quizId, Dictionary<int, string> answers);
        Task<bool> CanUserTakeQuiz(int userId, int quizId);

        // الوظائف الإضافية
        Task<QuizDetailsDto> GetQuizDetails(int quizId);
        Task<QuizDetailsDto> GetQuizForStudent(int quizId);
        Task<QuizResultDto> GetQuizResult(int resultId);
        Task<QuizDto> CreateQuiz(CreateQuizDto dto, int userId);
        Task<QuizStatsDto> GetQuizStatistics(int quizId);
        Task<bool> DeleteQuiz(int quizId);
    }
}