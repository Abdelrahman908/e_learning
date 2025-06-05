using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class PaymentRequestDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // تغيير من nullable إلى required

        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; } // إضافة اختيارية للمبلغ
    }
}
