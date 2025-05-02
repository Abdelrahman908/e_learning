using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace e_learning.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly string _emailTemplatePath;

        public EmailService(
            IConfiguration config,
            ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
            _emailTemplatePath = _config["EmailSettings:TemplatePath"] ?? "Templates/Email";
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
                var emailSettings = _config.GetSection("EmailSettings");

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(
                    emailSettings["SenderName"] ?? "نظام التعليم الإلكتروني",
                    emailSettings["SenderEmail"] ?? "noreply@elearning.com"
                ));
                emailMessage.To.Add(MailboxAddress.Parse(toEmail));
                emailMessage.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = StripHtml(body) // نسخة نصية للتوافق
                };

                emailMessage.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    emailSettings["SmtpServer"] ?? "smtp.gmail.com",
                    int.Parse(emailSettings["Port"] ?? "587"),
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    emailSettings["Username"],
                    emailSettings["Password"]
                );

                await smtp.SendAsync(emailMessage);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"تم إرسال البريد إلى {toEmail} بنجاح");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"فشل إرسال البريد إلى {toEmail}");
                return false;
            }
        }

        private string StripHtml(string html)
        {
            // تنفيذ بسيط لإزالة الوسوم HTML
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
        }
    }
}