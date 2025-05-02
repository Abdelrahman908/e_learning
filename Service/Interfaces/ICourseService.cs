using e_learning.DTOs;
using e_learning.DTOs.Courses;
namespace e_learning.Service.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDetailsDto>> GetCoursesAsync(CourseFilterDto filter);
        Task<CourseDetailsDto> GetCourseByIdAsync(int id);
        Task CreateCourseAsync(CourseCreateDto courseDto);
        Task UpdateCourseAsync(int id, CourseUpdateDto courseDto);
        Task DeleteCourseAsync(int id);
    }
}