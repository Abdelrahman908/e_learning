using e_learning.DTOs;
using e_learning.DTOs.e_learning.DTOs.Lessons;
using e_learning.DTOs.Responses;
using System.Security.Claims;

namespace e_learning.Service.Interfaces
{
    public interface ILessonService
    {
        Task<ApiResponse<List<LessonBriefDto>>> GetCourseLessons(int courseId, ClaimsPrincipal user);
        Task<ApiResponse<LessonResponseDto>> GetLessonDetails(int courseId, int lessonId, ClaimsPrincipal user);
        Task<ApiResponse<LessonResponseDto>> CreateLesson(int courseId, CreateLessonDto dto, int userId);
        Task<ApiResponse<LessonResponseDto>> UpdateLesson(int courseId, int lessonId, UpdateLessonDto dto);
        Task<ApiResponse> DeleteLesson(int courseId, int lessonId);
    }
}
