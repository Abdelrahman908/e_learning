using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace e_learning.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"] ?? throw new ArgumentNullException("SenderName"),
                _config["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException("SenderEmail")
            ));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(
                _config["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("SmtpServer"),
                int.Parse(_config["EmailSettings:Port"] ?? throw new ArgumentNullException("Port")),
                MailKit.Security.SecureSocketOptions.StartTls // SSL أفضل حماية
            );
            smtp.Authenticate(
                _config["EmailSettings:Username"] ?? throw new ArgumentNullException("Username"),
                _config["EmailSettings:Password"] ?? throw new ArgumentNullException("Password")
            );
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
