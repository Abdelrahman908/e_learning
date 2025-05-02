// في ملف Services/PasswordValidator.cs
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace e_learning.Services
{
    public class PasswordValidator : IPasswordValidator
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PasswordValidator> _logger;

        public PasswordValidator(IConfiguration config, ILogger<PasswordValidator> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool Validate(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            var minLength = _config.GetValue<int>("PasswordPolicy:MinLength", 8);
            if (password.Length < minLength)
            {
                errorMessage = $"يجب أن تتكون كلمة المرور من {minLength} أحرف على الأقل";
                _logger.LogWarning($"كلمة مرور ضعيفة: الطول أقل من {minLength}");
                return false;
            }

            // باقي التحققات...
            return true;
        }

        public bool IsStrongPassword(string password)
        {
            return Validate(password, out _);
        }
    }
}