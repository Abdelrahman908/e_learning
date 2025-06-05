using e_learning.DTOs;
using e_learning.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace e_learning.Controllers
{
    [ApiController]
    [Route("api/test-drive")]
    public class TestDriveController : ControllerBase
    {
        private readonly ILessonFileService _lessonFileService;

        public TestDriveController(ILessonFileService lessonFileService)
        {
            _lessonFileService = lessonFileService;
        }

        [HttpPost("upload")]
        [Authorize] // لو عندك JWT
        public async Task<IActionResult> UploadFile([FromForm] UploadMaterialDto dto, [FromQuery] int lessonId)
        {
            // مبدئياً نجرب بـ userId ثابت
            string uploadedById = "test-user-id";

            var result = await _lessonFileService.SaveLessonMaterialAsync(lessonId, dto, uploadedById);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
