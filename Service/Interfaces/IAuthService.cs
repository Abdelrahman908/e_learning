using e_learning.DTOs;
using e_learning.Models;

namespace e_learning.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(UserRegisterDto dto);
        Task<AuthResult> LoginAsync(UserLoginDto dto);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest dto);
        Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<AuthResult> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto);
    }
}