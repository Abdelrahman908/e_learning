using AutoMapper;
using e_learning.Data;
using e_learning.DTOs;
using e_learning.DTOs.Responses;
using e_learning.Models;
using e_learning.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace e_learning.Service.Implementations
{
    public class LessonFileService : ILessonFileService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly ILessonService _lessonService;

        public LessonFileService(
            AppDbContext context,
            IMapper mapper,
            IGoogleDriveService googleDriveService,
            ILessonService lessonService)
        {
            _context = context;
            _mapper = mapper;
            _googleDriveService = googleDriveService;
            _lessonService = lessonService;
        }

        public async Task<ApiResponse<LessonMaterialDto>> SaveLessonMaterialAsync(int lessonId, UploadMaterialDto dto, string uploadedById)
        {
            // التحقق من وجود الدرس
            var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == lessonId);
            if (!lessonExists)
                return ApiResponse<LessonMaterialDto>.NotFound("الدرس غير موجود");

            string driveFileId = await _googleDriveService.UploadFileAsync(dto.File, "YOUR_DRIVE_FOLDER_ID");
            string fileUrl = $"https://drive.google.com/file/d/{driveFileId}/view";

            var material = new LessonMaterial
            {
                FileName = dto.File.FileName,
                FileUrl = fileUrl,
                DriveFileId = driveFileId,
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

            if (!string.IsNullOrEmpty(material.DriveFileId))
            {
                await _googleDriveService.DeleteFileAsync(material.DriveFileId);
            }

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

            return ApiResponse<List<LessonMaterialDto>>.SuccessResponse(
                _mapper.Map<List<LessonMaterialDto>>(materials));
        }
    }
}