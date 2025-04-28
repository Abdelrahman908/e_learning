using e_learning.Data;
using e_learning.DTOs;
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

        // ✅ عرض مدفوعات المستخدم الحالي
        [Authorize(Roles = "Student")]
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("❌ المستخدم غير مصرح.");

            var userId = int.Parse(userIdClaim.Value);

            var payments = await _context.Payments
                .Include(p => p.Course)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaidAt)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.Id,
                    CourseId = p.CourseId,
                    CourseTitle = p.Course != null ? p.Course.Title : "غير معروف",
                    AmountPaid = p.Amount,
                    Method = p.PaymentMethod,
                    Success = p.IsSuccessful,
                    Date = p.PaidAt,
                    TransactionId = p.TransactionId
                })
                .ToListAsync();

            return Ok(payments);
        }
        [Authorize]
        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> CancelPayment(int paymentId)
        {
            // ✅ الحصول على معرف المستخدم الحالي من الـ Token
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return Unauthorized("لا يمكن تحديد المستخدم الحالي.");
            }

            // ✅ التحقق من وجود العملية وربطها بالمستخدم
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

            if (payment == null)
                return NotFound("العملية غير موجودة أو لا تتبع المستخدم الحالي.");

            // ✅ التحقق من حالة العملية إذا كانت ناجحة
            if (payment.IsSuccessful)
            {
                return BadRequest("لا يمكن إلغاء عملية ناجحة.");
            }

            // ✅ إلغاء العملية وإزالتها من قاعدة البيانات
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok("تم إلغاء العملية بنجاح ✅");
        }

        // 💳 تنفيذ عملية دفع لكورس
        [HttpPost("pay")]
        public async Task<IActionResult> MakePayment([FromBody] PaymentRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("❌ المستخدم غير مصرح.");

            var userId = int.Parse(userIdClaim.Value);

            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
                return NotFound("❌ الكورس غير موجود.");

            if (course.Price == 0)
                return BadRequest("🎓 الكورس مجاني ولا يحتاج إلى دفع.");

            // 🛑 تأكد من عدم التكرار في الدفع
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == course.Id);

            if (alreadyEnrolled)
                return BadRequest("⚠️ أنت بالفعل مسجل في هذا الكورس.");

            // ✅ محاكاة الدفع
            var payment = new Payment
            {
                UserId = userId,
                CourseId = course.Id,
                Amount = course.Price,
                PaymentMethod = dto.PaymentMethod,
                IsSuccessful = true,
                TransactionId = Guid.NewGuid().ToString(),
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = course.Id
            };

            _context.Enrollments.Add(enrollment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ تم الدفع بنجاح وتم تسجيلك بالكورس!",
                transactionId = payment.TransactionId
            });
        }
    }
}
