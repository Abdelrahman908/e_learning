namespace e_learning.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; } // ex: Visa, PayPal
        public bool IsSuccessful { get; set; } = false;

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public string? TransactionId { get; set; } // optional
    }
}
