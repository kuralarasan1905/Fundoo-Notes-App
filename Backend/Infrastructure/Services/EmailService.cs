using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Email service implementation
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("Sender email not configured");
            _senderPassword = _configuration["EmailSettings:SenderPassword"] ?? throw new InvalidOperationException("Sender password not configured");
            _senderName = _configuration["EmailSettings:SenderName"] ?? "Fundoo Notes";
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds timeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                _logger.LogInformation("Attempting to send email to {Email} via {SmtpServer}:{SmtpPort}", to, _smtpServer, _smtpPort);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}: {StatusCode} - {Message}", to, ex.StatusCode, ex.Message);
                throw new InvalidOperationException($"Failed to send email via SMTP: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }

        public async Task SendEmailVerificationAsync(string to, string verificationToken)
        {
            var subject = "Verify Your Email - Fundoo Notes";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Fundoo Notes!</h2>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='http://localhost:4200/verify-email?token={verificationToken}'>Verify Email</a></p>
                    <p>If you didn't create an account with us, please ignore this email.</p>
                    <p>Best regards,<br>Fundoo Notes Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetAsync(string to, string resetToken)
        {
            var subject = "Reset Your Password - Fundoo Notes";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>You requested to reset your password. Click the link below to reset it:</p>
                    <p><a href='http://localhost:4200/reset-password?token={resetToken}'>Reset Password</a></p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <p>Best regards,<br>Fundoo Notes Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendReminderNotificationAsync(string to, string noteTitle, DateTime reminderTime)
        {
            var subject = $"Reminder: {noteTitle}";
            var body = $@"
                <html>
                <body>
                    <h2>Note Reminder</h2>
                    <p>This is a reminder for your note: <strong>{noteTitle}</strong></p>
                    <p>Reminder time: {reminderTime:yyyy-MM-dd HH:mm}</p>
                    <p><a href='http://localhost:4200/notes'>View Your Notes</a></p>
                    <p>Best regards,<br>Fundoo Notes Team</p>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }
    }
}
