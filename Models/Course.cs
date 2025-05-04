using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using e_learning.models;
using e_learning.Models;

public class Course
{
    // Primary Key
    public int Id { get; set; }

    // Main Info
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

    // Foreign Keys
    [ForeignKey("Category")]
    public int CategoryId { get; set; }

    [ForeignKey("Instructor")]
    public int InstructorId { get; set; }

    public int CreatedBy { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Category Category { get; set; } = null!;
    public User Instructor { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
