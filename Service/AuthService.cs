using e_learning.Data;
using e_learning.DTOs;
using e_learning.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace e_learning.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly IPasswordValidator _passwordValidator;

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthService> logger,
            IPasswordValidator passwordValidator)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            _passwordValidator = passwordValidator;
        }

        public async Task<AuthResult> RegisterAsync(UserRegisterDto dto)
        {
            try
            {
                // التحقق من صحة كلمة المرور
                if (!_passwordValidator.Validate(dto.Password, out var passwordError))
                {
                    return new AuthResult { Message = passwordError, Success = false };
                }

                // التحقق من عدم وجود البريد مسبقاً
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    _logger.LogWarning("Duplicate email registration attempt: {Email}", dto.Email);
                    return new AuthResult { Message = "البريد الإلكتروني مسجل بالفعل", Success = false };
                }

                // إنشاء مستخدم جديد
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

                // إنشاء وإرسال كود التأكيد
                var confirmationCode = GenerateSixDigitCode();
                await StoreConfirmationCode(dto.Email, confirmationCode);

                var emailSent = await _emailService.SendConfirmationEmailAsync(dto.Email, confirmationCode);
                if (!emailSent)
                {
                    _logger.LogError("Failed to send confirmation email to {Email}", dto.Email);
                    // يمكنك التعامل مع حالة فشل الإرسال حسب احتياجاتك
                }

                _logger.LogInformation("New user registered - ID: {UserId}", user.Id);

                return new AuthResult
                {
                    Success = true,
                    Message = "تم التسجيل بنجاح، يرجى تأكيد بريدك الإلكتروني",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return new AuthResult { Message = "حدث خطأ أثناء التسجيل", Success = false };
            }
        }

        public async Task<AuthResult> LoginAsync(UserLoginDto dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                // التحقق من صحة بيانات الدخول
                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
                    return new AuthResult { Message = "بيانات الدخول غير صحيحة", Success = false };
                }

                // التحقق من تأكيد البريد الإلكتروني
                if (!user.IsEmailConfirmed)
                {
                    _logger.LogWarning("Login attempt with unconfirmed email: {Email}", dto.Email);
                    return new AuthResult { Message = "يجب تأكيد البريد الإلكتروني أولاً", Success = false };
                }

                // إنشاء التوكنات
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateSecureRefreshToken();

                // تخزين توكن التحديث
                await StoreRefreshToken(user.Id, refreshToken);

                _logger.LogInformation("Successful login for {Email}", dto.Email);

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresIn = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpiryInHours")),
                    Message = "تم تسجيل الدخول بنجاح",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new AuthResult { Message = "حدث خطأ أثناء تسجيل الدخول", Success = false };
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDto dto)
        {
            try
            {
                var refreshToken = await _context.UserRefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.RefreshToken == dto.RefreshToken);

                // التحقق من صلاحية توكن التحديث
                if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid refresh token attempt");
                    return new AuthResult { Message = "توكن التحديث غير صالح أو منتهي الصلاحية", Success = false };
                }

                var user = refreshToken.User;
                if (user == null)
                {
                    _logger.LogError("Refresh token with no associated user - Token: {TokenId}", refreshToken.Id);
                    return new AuthResult { Message = "المستخدم غير موجود", Success = false };
                }

                // إنشاء توكنات جديدة
                var newJwtToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateSecureRefreshToken();

                // إزالة التوكن القديم وإضافة الجديد
                _context.UserRefreshTokens.Remove(refreshToken);
                await StoreRefreshToken(user.Id, newRefreshToken);

                _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

                return new AuthResult
                {
                    Success = true,
                    Token = newJwtToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpiryInHours")),
                    Message = "تم تجديد التوكن بنجاح",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthResult { Message = "حدث خطأ أثناء تجديد التوكن", Success = false };
            }
        }

        public async Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            try
            {
                var isValidCode = await ValidateConfirmationCode(dto.Email, dto.Code);
                if (!isValidCode)
                {
                    return new AuthResult { Message = "كود التأكيد غير صحيح أو منتهي الصلاحية", Success = false };
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    return new AuthResult { Message = "المستخدم غير موجود", Success = false };
                }

                user.IsEmailConfirmed = true;
                await _context.SaveChangesAsync();
                await RemoveConfirmationCode(dto.Email);

                _logger.LogInformation("Email confirmed for: {Email}", dto.Email);

                return new AuthResult
                {
                    Success = true,
                    Message = "تم تأكيد البريد الإلكتروني بنجاح"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation");
                return new AuthResult { Message = "حدث خطأ أثناء تأكيد البريد", Success = false };
            }
        }

        public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Password reset request for unregistered email: {Email}", dto.Email);
                    return new AuthResult { Message = "البريد الإلكتروني غير مسجل", Success = false };
                }

                var resetCode = GenerateSixDigitCode();
                await StorePasswordResetCode(dto.Email, resetCode);

                var emailSent = await _emailService.SendPasswordResetEmailAsync(dto.Email, resetCode);
                if (!emailSent)
                {
                    _logger.LogError("Failed to send password reset email to {Email}", dto.Email);
                }

                _logger.LogInformation("Password reset code sent to: {Email}", dto.Email);

                return new AuthResult
                {
                    Success = true,
                    Message = "تم إرسال رمز إعادة التعيين إلى بريدك"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request");
                return new AuthResult { Message = "حدث خطأ أثناء طلب إعادة تعيين كلمة المرور", Success = false };
            }
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                // التحقق من صحة كلمة المرور الجديدة
                if (!_passwordValidator.Validate(dto.NewPassword, out var passwordError))
                {
                    return new AuthResult { Message = passwordError, Success = false };
                }

                // التحقق من صحة كود إعادة التعيين
                var isValidCode = await ValidatePasswordResetCode(dto.Email, dto.Code);
                if (!isValidCode)
                {
                    return new AuthResult { Message = "كود إعادة التعيين غير صحيح أو منتهي الصلاحية", Success = false };
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    return new AuthResult { Message = "المستخدم غير موجود", Success = false };
                }

                // تحديث كلمة المرور
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                // إبطال جميع توكنات التحديث القديمة
                await InvalidateUserRefreshTokens(user.Id);

                await _context.SaveChangesAsync();
                await RemovePasswordResetCode(dto.Email);

                _logger.LogInformation("Password reset for: {Email}", dto.Email);

                return new AuthResult
                {
                    Success = true,
                    Message = "تم تغيير كلمة المرور بنجاح"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return new AuthResult { Message = "حدث خطأ أثناء إعادة تعيين كلمة المرور", Success = false };
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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("fullName", user.FullName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key")));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpiryInHours", 2)),
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256)
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
            var codes = await _context.EmailConfirmationCodes
                .Where(e => e.Email == email.ToLower())
                .ToListAsync();

            if (codes.Any())
            {
                _context.EmailConfirmationCodes.RemoveRange(codes);
                await _context.SaveChangesAsync();
            }
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
            var codes = await _context.PasswordResetCodes
                .Where(p => p.Email == email.ToLower())
                .ToListAsync();

            if (codes.Any())
            {
                _context.PasswordResetCodes.RemoveRange(codes);
                await _context.SaveChangesAsync();
            }
        }

        private async Task StoreRefreshToken(int userId, string refreshToken)
        {
            await _context.UserRefreshTokens.AddAsync(new UserRefreshToken
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays", 7)),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task InvalidateUserRefreshTokens(int userId)
        {
            var tokens = await _context.UserRefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync();

            if (tokens.Any())
            {
                _context.UserRefreshTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }
        }

        #endregion
    }
}