using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace e_learning.Service.Interfaces
{
    public interface IGoogleDriveService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderId = null);
        Task DeleteFileAsync(string fileId);
        Task<string> GeneratePublicUrlAsync(string fileId);
        Task<long> GetFileSizeAsync(string fileId);
        Task<bool> FileExistsAsync(string fileId);
    }
}