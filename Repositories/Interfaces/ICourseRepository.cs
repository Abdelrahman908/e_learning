using e_learning.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace e_learning.Repositories.Interfaces
{
    public interface ICourseRepository
    {
       
        Task<IEnumerable<Course>> GetCoursesAsync();

    
        Task<Course> GetCourseByIdAsync(int id);

        
        Task AddCourseAsync(Course course);

       
        Task UpdateCourseAsync(Course course);

      
        Task DeleteCourseAsync(int id);
    }
}
