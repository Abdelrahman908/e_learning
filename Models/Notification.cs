using e_learning.Models;
using System.ComponentModel.DataAnnotations;

public class Notification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }  // تم تعديل النوع من string إلى int

    public string? SenderId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string NotificationType { get; set; } = "General";

    public User? User { get; set; }
}
