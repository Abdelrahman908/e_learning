using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using e_learning.Data;
using e_learning.Models;
using e_learning.DTOs;

namespace e_learning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔐 حماية جميع العمليات بالتوكن
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ إنشاء بروفايل
        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                if (await _context.Profiles.AnyAsync(p => p.UserId == userId))
                    return BadRequest("البروفايل موجود بالفعل.");

                var profile = new Profile
                {
                    UserId = userId,
                    Bio = dto.Bio,
                    ProfilePicture = dto.ProfilePicture,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();

                return Ok("تم إنشاء البروفايل ✅");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء إنشاء البروفايل: {ex.Message}");
            }
        }

        // ✅ عرض البروفايل الخاص بالمستخدم
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var profile = await _context.Profiles.Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                    return NotFound("البروفايل غير موجود ❌");

                var response = new ProfileResponseDto
                {
                    Id = profile.Id,
                    Bio = profile.Bio,
                    ProfilePicture = profile.ProfilePicture,
                    Address = profile.Address,
                    Phone = profile.Phone,
                    UserId = profile.UserId,
                    UserName = profile.User?.FullName
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء جلب البروفايل: {ex.Message}");
            }
        }

        // ✅ تعديل البروفايل
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                    return NotFound("البروفايل غير موجود.");

                profile.Bio = dto.Bio ?? profile.Bio;
                profile.ProfilePicture = dto.ProfilePicture ?? profile.ProfilePicture;
                profile.Address = dto.Address ?? profile.Address;
                profile.Phone = dto.Phone ?? profile.Phone;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok("تم تحديث البروفايل ✅");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء تحديث البروفايل: {ex.Message}");
            }
        }

        // ✅ حذف البروفايل
        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                    return NotFound("البروفايل غير موجود.");

                _context.Profiles.Remove(profile);
                await _context.SaveChangesAsync();

                return Ok("تم حذف البروفايل ✅");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء حذف البروفايل: {ex.Message}");
            }
        }

        // ✅ رفع صورة بروفايل
        [HttpPost("upload-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("الرجاء اختيار صورة.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest("امتداد الملف غير مسموح. الرجاء اختيار صورة بصيغة JPG أو PNG.");

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                    return NotFound("الملف الشخصي غير موجود.");

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var folderPath = Path.Combine("wwwroot", "images", "profiles");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                profile.ProfilePicture = $"/images/profiles/{fileName}";
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok("تم رفع الصورة بنجاح ✅");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء رفع الصورة: {ex.Message}");
            }
        }
    }
}