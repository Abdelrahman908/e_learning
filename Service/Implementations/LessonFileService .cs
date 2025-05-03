using AutoMapper;
using e_learning.Data;
using e_learning.DTOs;
using e_learning.DTOs.Responses;
using e_learning.Models;
using e_learning.Models.e_learning.Models;
using e_learning.Service.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace e_learning.Service.Implementations
{
    public class LessonFileService : ILessonFileService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public LessonFileService(AppDbContext context, IWebHostEnvironment env, IMapper mapper)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        public async Task<ApiResponse<LessonMaterialDto>> SaveLessonMaterialAsync(int lessonId, UploadMaterialDto dto, string uploadedById)
        {
            var fileUrl = await SaveFileAsync(dto.File, "lesson-materials");

            var material = new LessonMaterial
            {
                FileName = dto.File.FileName,
                FileUrl = fileUrl,
                Description = dto.Description,
                FileSize = dto.File.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedById = uploadedById,
                LessonId = lessonId
            };

            _context.LessonMaterials.Add(material);
            await _context.SaveChangesAsync();

            return ApiResponse<LessonMaterialDto>.SuccessResponse(_mapper.Map<LessonMaterialDto>(material));
        }

        public async Task<ApiResponse> DeleteLessonMaterialAsync(int materialId)
        {
            var material = await _context.LessonMaterials.FindAsync(materialId);
            if (material == null)
                return ApiResponse.NotFound("المادة غير موجودة");

            await DeleteFileAsync(material.FileUrl);
            _context.LessonMaterials.Remove(material);
            await _context.SaveChangesAsync();

            return ApiResponse.Ok("تم حذف المادة بنجاح");
        }

        public async Task<ApiResponse<LessonMaterialDto>> GetMaterialDetailsAsync(int materialId)
        {
            var material = await _context.LessonMaterials.FindAsync(materialId);
            if (material == null)
                return ApiResponse<LessonMaterialDto>.NotFound("المادة غير موجودة");

            return ApiResponse<LessonMaterialDto>.SuccessResponse(_mapper.Map<LessonMaterialDto>(material));
        }

        public async Task<ApiResponse<List<LessonMaterialDto>>> GetLessonMaterialsAsync(int lessonId)
        {
            var materials = await _context.LessonMaterials
                .Where(m => m.LessonId == lessonId)
                .ToListAsync();

            return ApiResponse<List<LessonMaterialDto>>.SuccessResponse(_mapper.Map<List<LessonMaterialDto>>(materials));
        }
    }
}
