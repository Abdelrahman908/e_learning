using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using e_learning.Service.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace e_learning.Controllers
{
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;

        public ProgressController(IProgressService progressService)
        {
            _progressService = progressService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserProgress(string userId)
        {
            // التحقق من صحة userId وتحويله إلى int
            if (!int.TryParse(userId, out int userIdInt))
            {
                return BadRequest("معرف المستخدم يجب أن يكون رقمًا صحيحًا");
            }

            // التحقق من الصلاحيات
            if (User.FindFirstValue(ClaimTypes.NameIdentifier) != userId && !User.IsInRole("Admin"))
                return Forbid();

            var response = await _progressService.GetUserProgress(userIdInt);
            return Ok(response);
        }

        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetCourseProgress(string userId, int courseId)
        {
            if (!int.TryParse(userId, out int userIdInt))
            {
                return BadRequest("معرف المستخدم يجب أن يكون رقمًا صحيحًا");
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) != userId && !User.IsInRole("Admin"))
                return Forbid();

            var response = await _progressService.GetCourseProgress(courseId, userIdInt);
            return Ok(response);
        }

        [HttpGet("lessons/{lessonId}")]
        public async Task<IActionResult> GetLessonProgress(string userId, int lessonId)
        {
            if (!int.TryParse(userId, out int userIdInt))
            {
                return BadRequest("معرف المستخدم يجب أن يكون رقمًا صحيحًا");
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) != userId && !User.IsInRole("Admin"))
                return Forbid();

            var response = await _progressService.GetLessonProgress(lessonId, userIdInt);
            return Ok(response);
        }
    }
}