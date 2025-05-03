using System;
using System.Collections.Generic;
using e_learning.models;

namespace e_learning.Models
{
    public class User
    {
        public int Id { get; set; }  // تغيير من Guid إلى int

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string Role { get; set; }  // Admin, Instructor, Student

        public Profile? Profile { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();  // الدورات التي يدرسها المدرب
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
    }
}