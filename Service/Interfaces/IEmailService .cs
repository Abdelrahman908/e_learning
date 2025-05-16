using System.Threading.Tasks;

namespace e_learning.Service.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationEmailAsync(string email, string confirmationCode);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetCode);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}