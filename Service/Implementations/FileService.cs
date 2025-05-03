using e_learning.Data;
using e_learning.DTOs;
using e_learning.Models;
using e_learning.Models.e_learning.Models;
using e_learning.Service.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace e_learning.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        public FileService(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            _context = context;
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

        public async Task<LessonMaterialDto> SaveLessonMaterialAsync(
            int lessonId,
            UploadMaterialDto dto,
            string uploadedById)
        {
            var fileUrl = await SaveFileAsync(dto.File, "materials");

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

            return MapToDto(material);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        public async Task DeleteLessonMaterialAsync(int materialId)
        {
            var material = await _context.LessonMaterials.FindAsync(materialId);
            if (material != null)
            {
                await DeleteFileAsync(material.FileUrl);
                _context.LessonMaterials.Remove(material);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<LessonMaterialDto> GetMaterialDetailsAsync(int materialId)
        {
            var material = await _context.LessonMaterials.FindAsync(materialId);
            return material != null ? MapToDto(material) : null;
        }

        public async Task<List<LessonMaterialDto>> GetLessonMaterialsAsync(int lessonId)
        {
            return await _context.LessonMaterials
                .Where(m => m.LessonId == lessonId)
                .Select(m => MapToDto(m))
                .ToListAsync();
        }

        private LessonMaterialDto MapToDto(LessonMaterial material)
        {
            return new LessonMaterialDto
            {
                Id = material.Id,
                FileName = material.FileName,
                FileUrl = material.FileUrl,
                Description = material.Description,
                FileSize = material.FileSize,
                UploadedAt = material.UploadedAt,
                LessonId = material.LessonId
            };
        }
    }
}