using e_learning.Data;
using e_learning.DTOs;
using e_learning.Models;
using e_learning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using e_learning.DTOs.Responses;

namespace e_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;
        private readonly IPasswordValidator _passwordValidator;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthController> logger,
            IPasswordValidator passwordValidator)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            _passwordValidator = passwordValidator;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("بيانات تسجيل غير صالحة: {@Dto}", dto);
                    return BadRequest(new ApiResponse(false, "البيانات غير مكتملة أو غير صالحة"));
                }

                if (!_passwordValidator.Validate(dto.Password, out var passwordError))
                {
                    return BadRequest(new ApiResponse(false, passwordError));
                }

                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    _logger.LogWarning("محاولة تسجيل بريد موجود: {Email}", dto.Email);
                    return Conflict(new ApiResponse(false, "البريد الإلكتروني مسجل بالفعل"));
                }

                var user = new User
                {
                    FullName = dto.FullName.Trim(),
                    Email = dto.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = NormalizeRole(dto.Role),
                    IsEmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var confirmationCode = GenerateSixDigitCode();
                await StoreConfirmationCode(dto.Email, confirmationCode);

                await _emailService.SendEmailAsync(
                    dto.Email,
                    "تأكيد البريد الإلكتروني",
                    $"<h2>رمز التأكيد: {confirmationCode}</h2><p>صالح لمدة 10 دقائق</p>"
                );

                _logger.LogInformation("تم تسجيل مستخدم جديد - ID: {UserId}", user.Id);

                return Ok(new ApiResponse<RegisterResponse>(true, "تم التسجيل بنجاح، يرجى تأكيد بريدك الإلكتروني",
                    new RegisterResponse { UserId = user.Id }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء التسجيل");
                return StatusCode(500, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("بيانات دخول غير صالحة: {@Dto}", dto);
                    return BadRequest(new ApiResponse(false, "البيانات غير مكتملة أو غير صالحة"));
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("محاولة دخول فاشلة للبريد: {Email}", dto.Email);
                    return Unauthorized(new ApiResponse(false, "بيانات الدخول غير صحيحة"));
                }

                if (!user.IsEmailConfirmed)
                {
                    _logger.LogWarning("محاولة دخول ببريد غير مؤكد: {Email}", dto.Email);
                    return Unauthorized(new ApiResponse(false, "يجب تأكيد البريد الإلكتروني أولاً"));
                }

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateSecureRefreshToken();

                await StoreRefreshToken(user.Id, refreshToken);

                _logger.LogInformation("تم تسجيل دخول ناجح لـ: {Email}", dto.Email);

                return Ok(new ApiResponse<LoginResponse>(true, "تم تسجيل الدخول بنجاح",
                    new LoginResponse
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        ExpiresIn = DateTime.UtcNow.AddHours(2),
                        User = new UserInfoDto
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Email = user.Email,
                            Role = user.Role
                        }
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تسجيل الدخول");
                return StatusCode(500, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        [HttpPost("confirm-email")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse(false, "البيانات غير مكتملة أو غير صالحة"));

                var isValidCode = await ValidateConfirmationCode(dto.Email, dto.Code);
                if (!isValidCode)
                    return BadRequest(new ApiResponse(false, "كود التأكيد غير صحيح أو منتهي الصلاحية"));

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return NotFound(new ApiResponse(false, "المستخدم غير موجود"));

                user.IsEmailConfirmed = true;
                await _context.SaveChangesAsync();
                await RemoveConfirmationCode(dto.Email);

                _logger.LogInformation("تم تأكيد البريد الإلكتروني لـ: {Email}", dto.Email);
                return Ok(new ApiResponse(true, "تم تأكيد البريد الإلكتروني بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تأكيد البريد");
                return StatusCode(500, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse(false, "البيانات غير مكتملة أو غير صالحة"));

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return NotFound(new ApiResponse(false, "البريد الإلكتروني غير مسجل"));

                var resetCode = GenerateSixDigitCode();
                await StorePasswordResetCode(dto.Email, resetCode);

                await _emailService.SendEmailAsync(
                    dto.Email,
                    "إعادة تعيين كلمة المرور",
                    $"<h2>رمز إعادة التعيين: {resetCode}</h2><p>صالح لمدة 10 دقائق</p>"
                );

                _logger.LogInformation("تم إرسال كود إعادة تعيين كلمة المرور لـ: {Email}", dto.Email);
                return Ok(new ApiResponse(true, "تم إرسال رمز إعادة التعيين إلى بريدك"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء طلب إعادة تعيين كلمة المرور");
                return StatusCode(500, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse(false, "البيانات غير مكتملة أو غير صالحة"));

                if (!_passwordValidator.Validate(dto.NewPassword, out var passwordError))
                {
                    return BadRequest(new ApiResponse(false, passwordError));
                }

                var isValidCode = await ValidatePasswordResetCode(dto.Email, dto.Code);
                if (!isValidCode)
                    return BadRequest(new ApiResponse(false, "كود إعادة التعيين غير صحيح أو منتهي الصلاحية"));

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return NotFound(new ApiResponse(false, "المستخدم غير موجود"));

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await InvalidateUserRefreshTokens(user.Id);
                await _context.SaveChangesAsync();
                await RemovePasswordResetCode(dto.Email);

                _logger.LogInformation("تم إعادة تعيين كلمة المرور لـ: {Email}", dto.Email);
                return Ok(new ApiResponse(true, "تم تغيير كلمة المرور بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء إعادة تعيين كلمة المرور");
                return StatusCode(500, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            try
            {
                var refreshToken = await _context.UserRefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.RefreshToken == dto.RefreshToken);

                if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("محاولة استخدام توكن تحديث غير صالح");
                    return Unauthorized(new ApiResponse(false, "توكن التحديث غير صالح أو منتهي الصلاحية"));
                }

                if (refreshToken.User == null)
                {
                    _logger.LogError("توكن التحديث بدون مستخدم مرتبط - Token: {TokenId}", refreshToken.Id);
                    return Unauthorized(new ApiResponse(false, "المستخدم غير موجود"));
                }

                var newJwtToken = GenerateJwtToken(refreshToken.User);
                var newRefreshToken = GenerateSecureRefreshToken();

                _context.UserRefreshTokens.Remove(refreshToken);
                await StoreRefreshToken(refreshToken.UserId, newRefreshToken);

                _logger.LogInformation("تم تجديد التوكن للمستخدم: {UserId}", refreshToken.UserId);

                return Ok(new ApiResponse<RefreshTokenResponse>(true, "تم تجديد التوكن بنجاح",
                    new RefreshTokenResponse
                    {
                        Token = newJwtToken,
                        RefreshToken = newRefreshToken
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تجديد التوكن");
                return StatusCode(500, new ApiResponse(false, "حدث خطأ أثناء معالجة طلبك"));
            }
        }

        #region Helper Methods

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpiryInHours")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateSecureRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static string GenerateSixDigitCode()
        {
            return new Random().Next(100000, 999999).ToString("D6");
        }

        private static string NormalizeRole(string? role)
        {
            return role?.Trim().ToLower() switch
            {
                "admin" => "Admin",
                "instructor" => "Instructor",
                _ => "Student"
            };
        }

        private async Task StoreConfirmationCode(string email, string code)
        {
            await _context.EmailConfirmationCodes.AddAsync(new EmailConfirmationCode
            {
                Email = email.ToLower(),
                Code = code,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10)
            });
            await _context.SaveChangesAsync();
        }

        private async Task<bool> ValidateConfirmationCode(string email, string code)
        {
            var record = await _context.EmailConfirmationCodes
                .Where(e => e.Email == email.ToLower() && e.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(e => e.ExpiryDate)
                .FirstOrDefaultAsync();

            return record != null && record.Code == code;
        }

        private async Task RemoveConfirmationCode(string email)
        {
            var codes = _context.EmailConfirmationCodes.Where(e => e.Email == email.ToLower());
            _context.EmailConfirmationCodes.RemoveRange(codes);
            await _context.SaveChangesAsync();
        }

        private async Task StorePasswordResetCode(string email, string code)
        {
            await _context.PasswordResetCodes.AddAsync(new PasswordResetCode
            {
                Email = email.ToLower(),
                Code = code,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10)
            });
            await _context.SaveChangesAsync();
        }

        private async Task<bool> ValidatePasswordResetCode(string email, string code)
        {
            var record = await _context.PasswordResetCodes
                .Where(p => p.Email == email.ToLower() && p.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(p => p.ExpiryDate)
                .FirstOrDefaultAsync();

            return record != null && record.Code == code;
        }

        private async Task RemovePasswordResetCode(string email)
        {
            var codes = _context.PasswordResetCodes.Where(p => p.Email == email.ToLower());
            _context.PasswordResetCodes.RemoveRange(codes);
            await _context.SaveChangesAsync();
        }

        private async Task StoreRefreshToken(int userId, string refreshToken)
        {
            await _context.UserRefreshTokens.AddAsync(new UserRefreshToken
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays")),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task InvalidateUserRefreshTokens(int userId)
        {
            var tokens = await _context.UserRefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync();

            _context.UserRefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}