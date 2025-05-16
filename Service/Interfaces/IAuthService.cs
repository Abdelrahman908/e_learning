using e_learning.DTOs;
using e_learning.DTOs.Responses;
using e_learning.Models;
using System.Threading.Tasks;

namespace e_learning.Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(UserRegisterDto dto);
        Task<AuthResult> LoginAsync(UserLoginDto dto);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest dto);
        Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<AuthResult> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResult> ResendConfirmationCodeAsync(string email);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto);
    }
}