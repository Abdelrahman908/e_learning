using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using e_learning.Service.Interfaces;

namespace e_learning.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendConfirmationEmailAsync(string email, string confirmationCode)
        {
            var subject = "تأكيد البريد الإلكتروني";
            var body = $"<h2>رمز التأكيد: {confirmationCode}</h2><p>صالح لمدة 10 دقائق</p>";
            return await SendEmailAsync(email, subject, body);

        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetCode)
        {
            var subject = "إعادة تعيين كلمة المرور";
            var body = $"<h2>رمز إعادة التعيين: {resetCode}</h2><p>صالح لمدة 10 دقائق</p>";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var settings = _config.GetSection("EmailSettings");
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    settings["SenderName"] ?? "E-Learning System",
                    settings["SenderEmail"] ?? "noreply@elearning.com"
                ));

                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = StripHtml(body)
                };

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    settings["SmtpServer"] ?? "smtp.gmail.com",
                    int.Parse(settings["Port"] ?? "587"),
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(settings["Username"], settings["Password"]);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }

        private string StripHtml(string html)
        {
            return Regex.Replace(html, "<[^>]*>", "");
        }
    }
}
