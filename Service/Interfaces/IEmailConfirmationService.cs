// في مجلد Services/IEmailConfirmationService.cs
using System.Threading.Tasks;

namespace e_learning.Services
{
    public interface IEmailConfirmationService
    {
        Task<string> GenerateAndStoreCode(string email);
        Task<bool> ValidateCode(string email, string code);
        Task RemoveCode(string email);
    }
}