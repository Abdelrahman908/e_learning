using System.ComponentModel.DataAnnotations.Schema;
using e_learning.models;
using e_learning.Models;

public class Course
{
    public int Id { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Level { get; set; }
    public int? DurationInMinutes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int InstructorId { get; set; }

    [ForeignKey("InstructorId")]
    public User Instructor { get; set; }  // المدرّس

    public ICollection<Enrollment>? Enrollments { get; set; }
    public ICollection<Payment>? Payments { get; set; }
    public ICollection<Review>? Reviews { get; set; }
    public ICollection<Lesson>? Lessons { get; set; }
    public ICollection<Quiz>? Quizzes { get; set; }
}
