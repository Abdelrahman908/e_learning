using System;
using System.ComponentModel.DataAnnotations.Schema;
using e_learning.models;
using e_learning.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }  // تغيير من Guid إلى int

    public Category Category { get; set; }

    public int InstructorId { get; set; }  // تغيير من Guid إلى int
    [ForeignKey("InstructorId")]
    public User? Instructor { get; set; }  // المدرّس

    public ICollection<Enrollment>? Enrollments { get; set; }
    public ICollection<Payment>? Payments { get; set; }
    public ICollection<Review>? Reviews { get; set; }
    public ICollection<Lesson>? Lessons { get; set; }
    public ICollection<Quiz>? Quizzes { get; set; }
}

