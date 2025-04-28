using e_learning.Models;
using e_learning.Data;
using System.Security.Cryptography;

namespace e_learning.Services
{
    public interface IAuthService
    {
        string GenerateRefreshToken();
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
