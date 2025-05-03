using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using e_learning.Enums;

namespace e_learning.Models
{
    public class QuizResult
    {
        public int UserId { get; set; }  // تغيير من string إلى int

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public int Id { get; set; }

        [Range(0, 100)]
        public int Score { get; set; } // النسبة المئوية (0-100)

        public bool IsPassed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan Duration => CompletedAt.HasValue ?
            CompletedAt.Value - StartedAt : TimeSpan.Zero;
        public QuizStatus Status { get; set; }

        // العلاقات
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new HashSet<UserAnswer>();
    }
}