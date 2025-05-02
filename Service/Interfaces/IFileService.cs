using e_learning.DTOs;
using e_learning.DTOs.Courses;
namespace e_learning.Service.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        Task DeleteFileAsync(string filePath);
    }
}
