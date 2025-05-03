using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using e_learning.Service.Interfaces;
using System.Security.Claims;
using e_learning.DTOs;
using e_learning.DTOs.Responses;
using e_learning.DTOs.e_learning.DTOs.Lessons;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace e_learning.Controllers
{
    [Route("api/courses/{courseId}/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILessonFileService _lessonFileService;

        public LessonsController(
            ILessonService lessonService,
            ILessonFileService lessonFileService)
        {
            _lessonService = lessonService;
            _lessonFileService = lessonFileService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<LessonBriefDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<LessonBriefDto>>>> GetLessons(int courseId)
        {
            var response = await _lessonService.GetCourseLessons(courseId, User);
            return HandleResponse(response);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LessonResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<LessonResponseDto>>> CreateLesson(
            int courseId,
            [FromBody] CreateLessonDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return BadRequest(ApiResponse<LessonResponseDto>.Error("معرف المستخدم غير صالح"));
            }

            var response = await _lessonService.CreateLesson(courseId, dto, userId);
            return HandleCreatedResponse(response, nameof(GetLesson), new { courseId });
        }

        [HttpGet("{lessonId}")]
        [ProducesResponseType(typeof(ApiResponse<LessonResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LessonResponseDto>>> GetLesson(
            int courseId,
            int lessonId)
        {
            var response = await _lessonService.GetLessonDetails(courseId, lessonId, User);
            return HandleResponse(response);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("{lessonId}/materials")]
        [RequestSizeLimit(50_000_000)]
        [ProducesResponseType(typeof(ApiResponse<LessonMaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LessonMaterialDto>>> UploadMaterial(
            int courseId,
            int lessonId,
            [FromForm] UploadMaterialDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _lessonFileService.SaveLessonMaterialAsync(lessonId, dto, userId);
            return HandleResponse(response);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{lessonId}")]
        [ProducesResponseType(typeof(ApiResponse<LessonResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<LessonResponseDto>>> UpdateLesson(
            int courseId,
            int lessonId,
            [FromBody] UpdateLessonDto dto)
        {
            var response = await _lessonService.UpdateLesson(courseId, lessonId, dto);
            return HandleResponse(response);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{lessonId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteLesson(int courseId, int lessonId)
        {
            var response = await _lessonService.DeleteLesson(courseId, lessonId);
            return HandleNoContentResponse(response);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("materials/{materialId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteMaterial(int courseId, int materialId)
        {
            var response = await _lessonFileService.DeleteLessonMaterialAsync(materialId);
            return HandleNoContentResponse(response);
        }

        #region Helper Methods
        private ActionResult HandleResponse<T>(ApiResponse<T> response)
        {
            // تحقق من Success وهي خاصية bool وليست دالة
            if (response == null || response.Success == false)
            {
                return StatusCode(response?.StatusCode ?? 400, response);
            }
            return Ok(response); // في حالة نجاح الاستجابة
        }

        private ActionResult HandleCreatedResponse<T>(
            ApiResponse<T> response,
            string actionName,
            object routeValues)
        {
            // تحقق من Success وهي خاصية bool وليست دالة
            if (response == null || response.Success == false)
            {
                return StatusCode(response?.StatusCode ?? 400, response);
            }
            return CreatedAtAction(actionName, routeValues, response); // في حالة نجاح الاستجابة
        }

        private ActionResult HandleNoContentResponse(ApiResponse response)
        {
            // تحقق من Success وهي خاصية bool وليست دالة
            if (response == null || response.Success == false)
            {
                return StatusCode(response?.StatusCode ?? 400, response);
            }
            return NoContent(); // في حالة نجاح الاستجابة
        }
        #endregion




    }
}