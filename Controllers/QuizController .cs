using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using e_learning.Data;
using e_learning.Models;
using e_learning.DTOs;
using System.Security.Claims;

namespace e_learning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizController(AppDbContext context)
        {
            _context = context;
        }

        // 🔨 إنشاء كويز مع أسئلة
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto dto)
        {
            var quiz = new Quiz
            {
                Title = dto.Title,
                CourseId = dto.CourseId,
                Questions = dto.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    Choices = q.Choices.Select(c => new Choice
                    {
                        Text = c.Text,
                        IsCorrect = c.IsCorrect
                    }).ToList()
                }).ToList()
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return Ok(new { message = "✅ تم إنشاء الكويز بنجاح", quiz.Id });
        }

        // 🔨 عرض كل الكويزات (Instructor/Student)
        [HttpGet]
        [Authorize(Roles = "Instructor, Student")]
        public async Task<IActionResult> GetAllQuizzes()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Instructor")
            {
                var quizzes = await _context.Quizzes
                    .Include(q => q.Course)
                    .Include(q => q.Questions)
                    .Include(q => q.QuizResults)
                    .Select(q => new
                    {
                        q.Id,
                        q.Title,
                        CourseTitle = q.Course.Title,
                        QuestionsCount = q.Questions.Count,
                        StudentsSubmitted = q.QuizResults.Count
                    })
                    .ToListAsync();

                return Ok(quizzes);
            }
            else
            {
                var quizzes = await _context.Quizzes
                    .Include(q => q.Course)
                    .Select(q => new
                    {
                        q.Id,
                        q.Title,
                        CourseTitle = q.Course.Title
                    })
                    .ToListAsync();

                return Ok(quizzes);
            }
        }

        // 🔨 عرض كويز بتفاصيل (Instructor/Student)
        [HttpGet("{id}")]
        [Authorize(Roles = "Instructor, Student")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound("❌ الكويز غير موجود.");

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Instructor")
            {
                var instructorView = new
                {
                    quiz.Id,
                    quiz.Title,
                    Questions = quiz.Questions.Select(q => new
                    {
                        q.Id,
                        q.Text,
                        Choices = q.Choices.Select(c => new
                        {
                            c.Id,
                            c.Text,
                            c.IsCorrect
                        })
                    })
                };
                return Ok(instructorView);
            }
            else
            {
                var studentView = new
                {
                    quiz.Id,
                    quiz.Title,
                    Questions = quiz.Questions.Select(q => new
                    {
                        q.Id,
                        q.Text,
                        Choices = q.Choices.Select(c => new
                        {
                            c.Id,
                            c.Text
                        })
                    })
                };
                return Ok(studentView);
            }
        }

        // 🔨 عرض نتائج الطلاب في كويز
        [HttpGet("{quizId}/results")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetQuizResults(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizResults)
                    .ThenInclude(qr => qr.User)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return NotFound("❌ الكويز غير موجود.");

            var results = quiz.QuizResults.Select(r => new
            {
                StudentName = r.User.FullName,
                r.TotalQuestions,
                r.CorrectAnswers,
                Score = $"{(r.CorrectAnswers * 100) / r.TotalQuestions}%"
            });

            return Ok(results);
        }

        // 🔨 بحث وفلترة في الكويزات
        [HttpGet("search")]
        [Authorize(Roles = "Instructor, Student")]
        public async Task<IActionResult> SearchQuizzes(string? keyword)
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Course)
                .Where(q => string.IsNullOrEmpty(keyword) || q.Title.Contains(keyword) || q.Course.Title.Contains(keyword))
                .Select(q => new
                {
                    q.Id,
                    q.Title,
                    CourseTitle = q.Course.Title
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        // 🔨 تعديل كويز
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] QuizCreateDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound("❌ الكويز غير موجود.");

            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
                return BadRequest("❌ الكورس غير موجود.");

            quiz.Title = dto.Title;
            quiz.CourseId = dto.CourseId;

            _context.Choices.RemoveRange(quiz.Questions.SelectMany(q => q.Choices));
            _context.Questions.RemoveRange(quiz.Questions);

            quiz.Questions = dto.Questions.Select(q => new Question
            {
                Text = q.Text,
                Choices = q.Choices.Select(c => new Choice
                {
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList()
            }).ToList();

            await _context.SaveChangesAsync();
            return Ok("✅ تم تعديل الكويز بنجاح.");
        }

        // 🔨 حذف كويز
        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
                return NotFound("❌ الكويز غير موجود.");

            _context.Choices.RemoveRange(quiz.Questions.SelectMany(q => q.Choices));
            _context.Questions.RemoveRange(quiz.Questions);
            _context.Quizzes.Remove(quiz);

            await _context.SaveChangesAsync();
            return Ok("✅ تم حذف الكويز بنجاح.");
        }
    }
}
