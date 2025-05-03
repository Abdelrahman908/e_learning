using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using e_learning.DTOs;
using e_learning.Service.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace e_learning.Controllers
{
    [Route("api/courses/{courseId}/lessons/{lessonId}/quizzes")]
    [ApiController]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizzesController> _logger;

        public QuizzesController(IQuizService quizService, ILogger<QuizzesController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        private bool TryGetUserId(out int userId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdString, out userId);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> CreateQuiz(
            int courseId,
            int lessonId,
            [FromBody] CreateQuizDto dto)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized("Invalid user ID format");
            }

            try
            {
                var quiz = await _quizService.CreateQuiz(dto, userId);
                return CreatedAtAction(
                    nameof(GetQuizDetails),
                    new { courseId, lessonId, quizId = quiz.Id },
                    quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz");
                return StatusCode(500, "An error occurred while creating the quiz");
            }
        }

        [HttpGet("{quizId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetQuizDetails(
            int courseId,
            int lessonId,
            int quizId)
        {
            try
            {
                var quiz = await _quizService.GetQuizDetails(quizId);
                return quiz == null ? NotFound("Quiz not found") : Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz details");
                return StatusCode(500, "An error occurred while retrieving quiz details");
            }
        }

        [HttpGet("{quizId}/student-view")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetQuizForStudent(
            int courseId,
            int lessonId,
            int quizId)
        {
            try
            {
                var quiz = await _quizService.GetQuizForStudent(quizId);
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student quiz view");
                return StatusCode(500, "An error occurred while retrieving quiz");
            }
        }

        [HttpPost("{quizId}/start")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StartQuiz(
            int courseId,
            int lessonId,
            int quizId)
        {
            if (!TryGetUserId(out int userId))
            {
                return BadRequest("Invalid user ID format");
            }

            try
            {
                var result = await _quizService.StartQuiz(userId, quizId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting quiz");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{quizId}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitQuiz(
            int courseId,
            int lessonId,
            int quizId,
            [FromBody] Dictionary<int, string> answers)
        {
            if (!TryGetUserId(out int userId))
            {
                return BadRequest("Invalid user ID format");
            }

            try
            {
                var result = await _quizService.SubmitQuiz(userId, quizId, answers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("results/{resultId}")]
        public async Task<IActionResult> GetQuizResult(
            int courseId,
            int lessonId,
            int resultId)
        {
            try
            {
                var result = await _quizService.GetQuizResult(resultId);
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz result");
                return StatusCode(500, "An error occurred while retrieving quiz result");
            }
        }

        [HttpGet("{quizId}/stats")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetQuizStatistics(
            int courseId,
            int lessonId,
            int quizId)
        {
            try
            {
                var stats = await _quizService.GetQuizStatistics(quizId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz statistics");
                return StatusCode(500, "An error occurred while retrieving quiz statistics");
            }
        }

        [HttpDelete("{quizId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> DeleteQuiz(
            int courseId,
            int lessonId,
            int quizId)
        {
            try
            {
                var success = await _quizService.DeleteQuiz(quizId);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz");
                return StatusCode(500, "An error occurred while deleting the quiz");
            }
        }
    }
}