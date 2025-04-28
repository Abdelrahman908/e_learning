namespace e_learning.DTOs
{
    public class PaymentRequestDto
    {
        public int CourseId { get; set; }
        public string? PaymentMethod { get; set; } // ex: Visa, Stripe
    }
}
