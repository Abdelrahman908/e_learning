using e_learning.models;
using e_learning.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Course
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "Unnamed Course";

    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Foreign Keys - Note CategoryId is nullable
    [ForeignKey("Category")]
    public int? CategoryId { get; set; }

    [ForeignKey("Instructor")]
    public int InstructorId { get; set; }

    public int CreatedBy { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Category? Category { get; set; }
    public virtual User Instructor { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}