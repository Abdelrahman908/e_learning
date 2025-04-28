using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "الإيميل مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكود مطلوب")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 حروف")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
