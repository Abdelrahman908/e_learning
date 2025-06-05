using e_learning.Data;
using e_learning.DTOs;
using e_learning.Enums;
using e_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace e_learning.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        // عرض المدفوعات مع Pagination
        [Authorize(Roles = "Student")]
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var query = _context.Payments
                .Include(p => p.Course)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaidAt);

            var totalPayments = await query.CountAsync();
            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.Id,
                    CourseId = p.CourseId,
                    CourseTitle = p.Course!.Title,
                    AmountPaid = p.Amount,
                    Method = p.PaymentMethod,
                    Status = p.Status.ToString(),
                    Date = p.PaidAt,
                    TransactionId = p.TransactionId
                })
                .ToListAsync();

            return Ok(new
            {
                Total = totalPayments,
                Page = page,
                PageSize = pageSize,
                Data = payments
            });
        }

        // إلغاء الدفع مع تحسينات
        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> CancelPayment(int paymentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId)
                ?? throw new KeyNotFoundException("عملية الدفع غير موجودة");

            if (payment.Status == PaymentStatus.Completed)
                return BadRequest(new { Message = "لا يمكن إلغاء عملية دفع مكتملة" });

            if (payment.PaidAt < DateTime.UtcNow.AddHours(-1))
                return BadRequest(new { Message = "انتهت فترة الإلغاء (ساعة واحدة)" });

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم الإلغاء بنجاح" });
        }

        // معالجة الدفع مع التحسينات
        [HttpPost("pay")]
        public async Task<IActionResult> MakePayment([FromBody] PaymentRequestDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var course = await _context.Courses.FindAsync(dto.CourseId)
                ?? throw new KeyNotFoundException("الكورس غير موجود");

            if (course.Price <= 0)
                return BadRequest(new { Message = "الكورس مجاني ولا يحتاج دفعًا" });

            if (await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == course.Id))
                return Conflict(new { Message = "مسجل بالفعل في هذا الكورس" });

            var payment = new Payment
            {
                UserId = userId,
                CourseId = course.Id,
                Amount = course.Price,
                PaymentMethod = dto.PaymentMethod ?? "Unknown",
                Status = PaymentStatus.Pending,
                TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}"
            };

            try
            {
                // محاكاة عملية الدفع
                var paymentResult = await ProcessPayment(payment);

                if (!paymentResult.Success)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = paymentResult.Message;
                    await _context.SaveChangesAsync();
                    return BadRequest(new { payment.FailureReason });
                }

                payment.Status = PaymentStatus.Completed;
                _context.Payments.Add(payment);

                _context.Enrollments.Add(new Enrollment
                {
                    UserId = userId,
                    CourseId = course.Id,
                    EnrolledAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "تم الدفع والتسجيل بنجاح",
                    payment.TransactionId,
                    Course = course.Title
                });
            }
            catch
            {
                payment.Status = PaymentStatus.Failed;
                await _context.SaveChangesAsync();
                throw;
            }
        }

        private async Task<PaymentResult> ProcessPayment(Payment payment)
        {
            await Task.Delay(500); // محاكاة اتصال ببوابة الدفع

            if (payment.Amount > 10000)
                return new PaymentResult { Success = false, Message = "المبلغ يتجاوز الحد المسموح" };

            return new PaymentResult { Success = true, Message = "تمت العملية بنجاح" };
        }

        private class PaymentResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}