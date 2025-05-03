using e_learning.DTOs;
using e_learning.DTOs.Responses;
using System.Threading.Tasks;

namespace e_learning.Service.Interfaces
{
    public interface IProgressService
    {
        Task<UserProgressStats> GetUserProgress(int userId);
        Task<CourseProgressDto> GetCourseProgress(int courseId, int userId);
        Task<LessonProgressDto> GetLessonProgress(int lessonId, int userId);
    }
}