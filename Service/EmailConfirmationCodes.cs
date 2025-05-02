// في مجلد Services/EmailConfirmationService.cs
using System;
using System.Threading.Tasks;
using e_learning.Data;
using e_learning.Models;
using Microsoft.EntityFrameworkCore;

namespace e_learning.Services
{
    public class EmailConfirmationService : IEmailConfirmationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmailConfirmationService> _logger;

        public EmailConfirmationService(
            AppDbContext context,
            ILogger<EmailConfirmationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateAndStoreCode(string email)
        {
            var code = new Random().Next(100000, 999999).ToString();

            await _context.EmailConfirmationCodes.AddAsync(new EmailConfirmationCode
            {
                Email = email.ToLower(),
                Code = code,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10)
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation($"تم إنشاء كود تأكيد للبريد {email}");
            return code;
        }

        public async Task<bool> ValidateCode(string email, string code)
        {
            var validCode = await _context.EmailConfirmationCodes
                .FirstOrDefaultAsync(c =>
                    c.Email == email.ToLower() &&
                    c.Code == code &&
                    c.ExpiryDate > DateTime.UtcNow);

            return validCode != null;
        }

        public async Task RemoveCode(string email)
        {
            var codes = await _context.EmailConfirmationCodes
                .Where(c => c.Email == email.ToLower())
                .ToListAsync();

            if (codes.Any())
            {
                _context.EmailConfirmationCodes.RemoveRange(codes);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"تم حذف أكواد تأكيد البريد لـ {email}");
            }
        }
    }
}