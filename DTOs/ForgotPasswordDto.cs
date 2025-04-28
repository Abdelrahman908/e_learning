using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "الإيميل مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; } = string.Empty;
    }
}
