namespace e_learning.DTOs
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } // تغيير من nullable إلى non-nullable
        public decimal AmountPaid { get; set; }
        public string Method { get; set; }

        public string PaymentMethod { get; set; } // تغيير من nullable إلى non-nullable
        public string Status { get; set; } // إضافة حالة الدفع
        public DateTime Date { get; set; }

        public string TransactionId { get; set; } // تغيير من nullable إلى non-nullable
    }
}
