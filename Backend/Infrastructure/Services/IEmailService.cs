namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Interface for email service
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendEmailVerificationAsync(string to, string verificationToken);
        Task SendPasswordResetAsync(string to, string resetToken);
        Task SendReminderNotificationAsync(string to, string noteTitle, DateTime reminderTime);
    }
}
