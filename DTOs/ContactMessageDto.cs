using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class ContactMessageDto
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الإيميل مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "نص الرسالة مطلوب")]
        [MinLength(10, ErrorMessage = "الرسالة يجب أن تكون أطول")]
        public string MessageText { get; set; } = string.Empty;
    }
}
