namespace e_learning.DTOs
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int CourseId { get; set; }
        public string? CourseTitle { get; set; } 
        public decimal AmountPaid { get; set; }
        public string? Method { get; set; } 
        public bool Success { get; set; }
        public DateTime Date { get; set; }
        public string? TransactionId { get; set; }  
    }
}
