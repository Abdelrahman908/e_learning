using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace e_learning.Service.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        Task DeleteFileAsync(string filePath);
    }
}
