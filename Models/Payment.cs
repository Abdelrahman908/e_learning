using System;
using e_learning.Enums;

namespace e_learning.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // تغيير من nullable إلى non-nullable
        public bool IsSuccessful { get; set; } = false;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending; // إضافة enum
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public string TransactionId { get; set; } // تغيير من nullable إلى non-nullable
        public string? FailureReason { get; set; } // إضافة سبب الفشل
    }
}
