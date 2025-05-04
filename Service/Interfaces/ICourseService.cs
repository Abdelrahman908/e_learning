using e_learning.DTOs.Courses;  // استيراد فقط المسار المناسب لـ DTOs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace e_learning.Service.Interfaces
{
    public interface ICourseService
    {
        // الدالة للحصول على قائمة الدورات مع فلترة
        Task<IEnumerable<CourseDetailsDto>> GetCoursesAsync(CourseFilterDto filter);

        // الدالة للحصول على تفاصيل دورة بناءً على المعرف
        Task<CourseDetailsDto> GetCourseByIdAsync(int id);

        // الدالة لإنشاء دورة جديدة
        Task CreateCourseAsync(CourseCreateDto courseDto);

        // الدالة لتحديث الدورة
        Task UpdateCourseAsync(int id, CourseUpdateDto courseDto);

        // الدالة لحذف دورة
        Task DeleteCourseAsync(int id);
    }
}
