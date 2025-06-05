using e_learning.DTOs;
using e_learning.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace e_learning.Service.Interfaces
{
    public interface ILessonFileService
    {
        Task<ApiResponse<LessonMaterialDto>> SaveLessonMaterialAsync(int lessonId, UploadMaterialDto dto, string uploadedById);
        Task<ApiResponse> DeleteLessonMaterialAsync(int materialId);
        Task<ApiResponse<LessonMaterialDto>> GetMaterialDetailsAsync(int materialId);
        Task<ApiResponse<List<LessonMaterialDto>>> GetLessonMaterialsAsync(int lessonId);
    }
}
