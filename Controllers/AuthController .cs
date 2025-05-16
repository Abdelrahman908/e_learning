using e_learning.Data;
using e_learning.DTOs;
using e_learning.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using e_learning.Models;
using e_learning.DTOs.Responses;
using e_learning.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using RefreshTokenResponse = e_learning.DTOs.Responses.RefreshTokenResponse;

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
        private readonly ITokenService _tokenService;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthController> logger,
            IPasswordValidator passwordValidator,
            ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            _passwordValidator = passwordValidator;
            _tokenService = tokenService;
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
                    _logger.LogWarning("Invalid registration data: {@Dto}", dto);
                    return BadRequest(new ApiResponse(false, "Incomplete or invalid data"));
                }

                if (!_passwordValidator.Validate(dto.Password, out var passwordError))
                {
                    return BadRequest(new ApiResponse(false, passwordError));
                }

                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    _logger.LogWarning("Attempt to register existing email: {Email}", dto.Email);
                    return Conflict(new ApiResponse(false, "Email already registered"));
                }

                var user = new User
                {
                    FullName = dto.FullName.Trim(),
                    Email = dto.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = NormalizeRole(dto.Role),
                    IsEmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var confirmationCode = GenerateSixDigitCode();
                await StoreConfirmationCode(dto.Email, confirmationCode);

                await _emailService.SendEmailAsync(
                    dto.Email,
                    "Email Confirmation",
                    $"<h2>Confirmation Code: {confirmationCode}</h2><p>Valid for 10 minutes</p>"
                );

                _logger.LogInformation("New user registered - ID: {UserId}", user.Id);

                return Ok(new ApiResponse<RegisterResponse>(true, "Registration successful, please confirm your email",
                    new RegisterResponse { UserId = user.Id }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid login data: {@Dto}", dto);
                    return BadRequest(new ApiResponse(false, "Incomplete or invalid data"));
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
                    return Unauthorized(new ApiResponse(false, "Invalid credentials"));
                }

                if (!user.IsEmailConfirmed)
                {
                    _logger.LogWarning("Login attempt with unconfirmed email: {Email}", dto.Email);
                    return Unauthorized(new ApiResponse(false, "Email confirmation required"));
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for deactivated account: {Email}", dto.Email);
                    return StatusCode(403, new ApiResponse(false, "Account is deactivated"));
                }

                var token = _tokenService.GenerateJwtToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                await StoreRefreshToken(user.Id, refreshToken);
                _logger.LogInformation("Successful login for: {Email}", dto.Email);

                return Ok(new ApiResponse<LoginResponse>(true, "Login successful",
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
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
            }
        }
        [HttpPost("resend-confirmation-code")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ResendConfirmationCode([FromBody] EmailDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid email format for resend confirmation: {Email}", dto.Email);
                    return BadRequest(new ApiResponse(false, "Invalid email format"));
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

                if (user == null)
                {
                    _logger.LogWarning("Resend confirmation attempt for non-existent user: {Email}", dto.Email);
                    return NotFound(new ApiResponse(false, "User not found"));
                }

                if (user.IsEmailConfirmed)
                {
                    _logger.LogWarning("Resend confirmation attempt for already confirmed email: {Email}", dto.Email);
                    return BadRequest(new ApiResponse(false, "Email is already confirmed"));
                }

                // Generate new confirmation code
                var confirmationCode = GenerateSixDigitCode();

                // Remove any existing codes and store the new one
                await RemoveConfirmationCode(dto.Email);
                await StoreConfirmationCode(dto.Email, confirmationCode);

                // Send confirmation email
                var emailSent = await _emailService.SendConfirmationEmailAsync(dto.Email, confirmationCode);
                if (!emailSent)
                {
                    _logger.LogError("Failed to send confirmation email to {Email}", dto.Email);
                    return StatusCode(500, new ApiResponse(false, "Failed to send confirmation email"));
                }

                _logger.LogInformation("Confirmation code resent successfully to: {Email}", dto.Email);
                return Ok(new ApiResponse(true, "Confirmation code resent successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending confirmation code to {Email}", dto.Email);
                return StatusCode(500, new ApiResponse(false, "An error occurred while resending confirmation code"));
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
                    return BadRequest(new ApiResponse(false, "Incomplete or invalid data"));

                var isValidCode = await ValidateConfirmationCode(dto.Email, dto.Code);
                if (!isValidCode)
                    return BadRequest(new ApiResponse(false, "Invalid or expired confirmation code"));

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return NotFound(new ApiResponse(false, "User not found"));

                user.IsEmailConfirmed = true;
                await _context.SaveChangesAsync();
                await RemoveConfirmationCode(dto.Email);

                _logger.LogInformation("Email confirmed for: {Email}", dto.Email);
                return Ok(new ApiResponse(true, "Email confirmed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
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
                    return BadRequest(new ApiResponse(false, "Incomplete or invalid data"));

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return NotFound(new ApiResponse(false, "Email not registered"));

                if (!user.IsEmailConfirmed)
                    return BadRequest(new ApiResponse(false, "Email not confirmed"));

                var resetCode = GenerateSixDigitCode();
                await StorePasswordResetCode(dto.Email, resetCode);

                await _emailService.SendEmailAsync(
                    dto.Email,
                    "Password Reset",
                    $"<h2>Reset Code: {resetCode}</h2><p>Valid for 10 minutes</p>"
                );

                _logger.LogInformation("Password reset code sent to: {Email}", dto.Email);
                return Ok(new ApiResponse(true, "Reset code sent to your email"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
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
                    return BadRequest(new ApiResponse(false, "Incomplete or invalid data"));

                if (!_passwordValidator.Validate(dto.NewPassword, out var passwordError))
                {
                    return BadRequest(new ApiResponse(false, passwordError));
                }

                var isValidCode = await ValidatePasswordResetCode(dto.Email, dto.Code);
                if (!isValidCode)
                    return BadRequest(new ApiResponse(false, "Invalid or expired reset code"));

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return NotFound(new ApiResponse(false, "User not found"));

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await InvalidateUserRefreshTokens(user.Id);
                await _context.SaveChangesAsync();
                await RemovePasswordResetCode(dto.Email);

                _logger.LogInformation("Password reset for: {Email}", dto.Email);
                return Ok(new ApiResponse(true, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new ApiResponse(false, "Refresh token is required"));
                }

                var refreshToken = await _context.UserRefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.RefreshToken == request.RefreshToken);

                if (refreshToken == null)
                {
                    return Unauthorized(new ApiResponse(false, "Invalid refresh token"));
                }

                if (refreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    return Unauthorized(new ApiResponse(false, "Refresh token expired"));
                }

                if (refreshToken.User == null || !refreshToken.User.IsActive)
                {
                    _logger.LogError("Refresh token {TokenId} has no associated user or user is inactive", refreshToken.Id);
                    return Unauthorized(new ApiResponse(false, "User not found or inactive"));
                }

                var newJwtToken = _tokenService.GenerateJwtToken(refreshToken.User);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Invalidate old refresh token
                refreshToken.IsUsed = true;
                _context.UserRefreshTokens.Update(refreshToken);

                // Store new refresh token
                await StoreRefreshToken(refreshToken.User.Id, newRefreshToken);
                await _context.SaveChangesAsync();

                // Calculate expiration in seconds
                var expiresIn = (int)(DateTime.UtcNow.AddHours(2) - DateTime.UtcNow).TotalSeconds;

                return Ok(new ApiResponse<RefreshTokenResponse>(true, "Token refreshed successfully",
                    new RefreshTokenResponse
                    {
                        Token = newJwtToken,
                        RefreshToken = newRefreshToken,
                        ExpiresIn = (int)TimeSpan.FromHours(2).TotalSeconds

                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                    return BadRequest(new ApiResponse(false, "Refresh token is required"));

                var refreshToken = await _context.UserRefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.RefreshToken == request.RefreshToken);

                if (refreshToken == null)
                    return Unauthorized(new ApiResponse(false, "Invalid refresh token"));

                if (refreshToken.User == null)
                    return Unauthorized(new ApiResponse(false, "User not found"));

                await InvalidateUserRefreshTokens(refreshToken.User.Id);

                _logger.LogInformation("User {UserId} logged out successfully", refreshToken.User.Id);
                return Ok(new ApiResponse(true, "Logged out successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
            }
        }


        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse(false, "Incomplete or invalid data"));

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new ApiResponse(false, "Invalid token"));
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not found"));
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new ApiResponse(false, "Current password is incorrect"));
                }

                if (!_passwordValidator.Validate(dto.NewPassword, out var passwordError))
                {
                    return BadRequest(new ApiResponse(false, passwordError));
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await InvalidateUserRefreshTokens(user.Id);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed for user {UserId}", userId);
                return Ok(new ApiResponse(true, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new ApiResponse(false, "An error occurred while processing your request"));
            }
        }

        #region Helper Methods

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
            // Remove any existing codes for this email first
            await RemoveConfirmationCode(email);

            await _context.EmailConfirmationCodes.AddAsync(new EmailConfirmationCode
            {
                Email = email.ToLower(),
                Code = code,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow
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
            // Remove any existing codes for this email first
            await RemovePasswordResetCode(email);

            await _context.PasswordResetCodes.AddAsync(new PasswordResetCode
            {
                Email = email.ToLower(),
                Code = code,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow
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
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays")),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            });
            await _context.SaveChangesAsync();
        }

        private async Task InvalidateUserRefreshTokens(int userId)
        {
            var tokens = await _context.UserRefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}