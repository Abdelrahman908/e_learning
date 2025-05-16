using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace e_learning.Services
{
    public class EmailConfirmationService : IEmailConfirmationService
    {
        private readonly IMemoryCache _cache;

        public EmailConfirmationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<string> GenerateAndStoreCode(string email)
        {
            var code = new Random().Next(100000, 999999).ToString();
            _cache.Set($"confirmation_code_{email}", code, TimeSpan.FromMinutes(10));
            return Task.FromResult(code);
        }

        public Task<bool> ValidateCode(string email, string code)
        {
            var cachedCode = _cache.Get<string>($"confirmation_code_{email}");
            return Task.FromResult(cachedCode == code);
        }

        public Task RemoveCode(string email)
        {
            _cache.Remove($"confirmation_code_{email}");
            return Task.CompletedTask;
        }
    }
}
