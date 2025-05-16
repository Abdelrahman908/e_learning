using e_learning.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class UserRefreshToken
{
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }  // تغيير من string إلى int

    public User User { get; set; } = null!;

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; }   // ✅ Add this
    public bool IsRevoked { get; set; } // ✅ Optional but commonly used

    public int ExpiresIn { get; set; }
}