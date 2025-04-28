using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using e_learning.Data;
using e_learning.DTOs;
using e_learning.Models;
using e_learning.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private static readonly Dictionary<string, string> emailConfirmations = new();

        public AuthController(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        // 🧩 Register
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "البيانات غير مكتملة ❌" });

            if (_context.Users.Any(u => u.Email == model.Email))
                return BadRequest(new { message = "الإيميل مسجل بالفعل ❌" });

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = hashedPassword,
                Role = model.Role?.Trim().ToLower() == "instructor" ? "Instructor"
                      : model.Role?.Trim().ToLower() == "admin" ? "Admin"
                      : "Student",
                IsEmailConfirmed = false
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // ارسال كود التأكيد
            var confirmationCode = GenerateSixDigitCode();
            emailConfirmations[model.Email] = confirmationCode;

            var subject = "كود تأكيد البريد الإلكتروني";
            var body = $"<h2>رمز تأكيد البريد الإلكتروني الخاص بك هو:</h2><h3>{confirmationCode}</h3><p>الرمز صالح لفترة محدودة.</p>";
            _emailService.SendEmail(model.Email, subject, body);

            return Ok(new { message = "تم التسجيل بنجاح ✅، برجاء تأكيد بريدك الإلكتروني 📧" });
        }

        // 🧩 Login
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "البيانات غير مكتملة ❌" });

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                return Unauthorized(new { message = "بيانات الدخول غير صحيحة ❌" });

            if (!user.IsEmailConfirmed)
                return Unauthorized(new { message = "برجاء تأكيد البريد الإلكتروني أولاً ❌" });

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            _context.UserRefreshTokens.Add(new UserRefreshToken
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                ExpiryDate = refreshTokenExpiry
            });
            _context.SaveChanges();

            return Ok(new
            {
                message = "تم تسجيل الدخول بنجاح ✅",
                token,
                refreshToken,
                user = new { user.Id, user.FullName, user.Email, user.Role }
            });
        }

        // 🧩 Get Profile
        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود ❌" });

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role
            });
        }

        // 🧩 Confirm Email
        [HttpPost("confirm-email")]
        public IActionResult ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "البيانات غير مكتملة ❌" });

            if (!emailConfirmations.TryGetValue(model.Email, out var code) || code != model.Code)
                return BadRequest(new { message = "الرمز غير صحيح أو منتهي ❌" });

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود ❌" });

            user.IsEmailConfirmed = true;
            _context.SaveChanges();
            emailConfirmations.Remove(model.Email);

            return Ok(new { message = "تم تأكيد البريد الإلكتروني بنجاح ✅" });
        }

        // 🧩 Forgot Password
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "البيانات غير مكتملة ❌" });

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود ❌" });

            var resetCode = GenerateSixDigitCode();
            emailConfirmations[model.Email] = resetCode;

            var subject = "كود إعادة تعيين كلمة المرور";
            var body = $"<h2>رمز إعادة تعيين كلمة المرور الخاص بك هو:</h2><h3>{resetCode}</h3><p>الرمز صالح لفترة محدودة.</p>";
            _emailService.SendEmail(model.Email, subject, body);

            return Ok(new { message = "تم إرسال رمز إعادة تعيين كلمة المرور إلى بريدك الإلكتروني 📧" });
        }

        // 🧩 Reset Password
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "البيانات غير مكتملة ❌" });

            if (!emailConfirmations.TryGetValue(model.Email, out var code) || code != model.Code)
                return BadRequest(new { message = "الرمز غير صحيح أو منتهي ❌" });

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود ❌" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();
            emailConfirmations.Remove(model.Email);

            return Ok(new { message = "تم تغيير كلمة المرور بنجاح ✅" });
        }

        // 🧩 Refresh Token
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenDto model)
        {
            var existingToken = _context.UserRefreshTokens.FirstOrDefault(t => t.RefreshToken == model.RefreshToken);
            if (existingToken == null || existingToken.ExpiryDate < DateTime.UtcNow)
                return Unauthorized(new { message = "التوكن غير صالح أو منتهي ❌" });

            var user = _context.Users.FirstOrDefault(u => u.Id == existingToken.UserId);
            if (user == null)
                return Unauthorized(new { message = "المستخدم غير موجود ❌" });

            var newJwtToken = GenerateJwtToken(user);

            return Ok(new { token = newJwtToken });
        }

        // 📦 Helpers

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "Student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private string GenerateSixDigitCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
