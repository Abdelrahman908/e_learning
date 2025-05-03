using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using e_learning.Enums;
using e_learning.Enums.e_learning.Enums;
using e_learning.Models.e_learning.Models;

namespace e_learning.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public LessonType Type { get; set; }

        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public string PdfUrl { get; set; }
        public int Duration { get; set; } // بالدقائق
        public bool IsFree { get; set; }
        public bool IsSequential { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }  // تغيير من string إلى int

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public Quiz Quiz { get; set; }
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public ICollection<LessonMaterial> Materials { get; set; } = new List<LessonMaterial>();
        public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
    }
}