using System;
using System.Collections.Generic;
using e_learning.Enums;

namespace e_learning.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public string Title { get; set; }
        public string Description { get; set; }
        public int PassingScore { get; set; }
        public bool IsMandatory { get; set; }
        public int MaxAttempts { get; set; }
        public int TimeLimitMinutes { get; set; } = 30;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }  // تغيير من string إلى int
        public QuizStatus Status { get; set; } = QuizStatus.Active;

        // العلاقات
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizResult> Results { get; set; } = new List<QuizResult>();
    }
}