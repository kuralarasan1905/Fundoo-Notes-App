using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;

namespace fundoo_notes.Application.Features.Users.Commands.DeactivateAccount
{
    /// <summary>
    /// Handler for DeactivateAccountCommand
    /// </summary>
    public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IEmailService _emailService;
        private readonly ILogger<DeactivateAccountCommandHandler> _logger;

        public DeactivateAccountCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ITokenBlacklistService tokenBlacklistService,
            IEmailService emailService,
            ILogger<DeactivateAccountCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _tokenBlacklistService = tokenBlacklistService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing account deactivation for user: {UserId}", request.UserId);

            // Get user by ID
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Account deactivation failed: User not found: {UserId}", request.UserId);
                throw new ArgumentException("User not found");
            }

            // Check if user is already inactive
            if (!user.IsActive)
            {
                _logger.LogWarning("Account deactivation failed: User account is already inactive: {UserId}", request.UserId);
                return true; // Already deactivated
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Account deactivation failed: Invalid password for user: {UserId}", request.UserId);
                throw new ArgumentException("Invalid password");
            }

            // Deactivate account
            user.IsActive = false;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // Blacklist all user tokens
            await _tokenBlacklistService.BlacklistUserTokensAsync(request.UserId);

            _logger.LogInformation("Account deactivated successfully for user: {UserId}, Reason: {Reason}", 
                request.UserId, request.Reason);

            // Send deactivation confirmation email
            try
            {
                await SendDeactivationEmailAsync(user.Email, user.FirstName, request.Reason);
                _logger.LogInformation("Deactivation confirmation email sent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send deactivation confirmation email to: {Email}", user.Email);
                // Don't fail the operation if email sending fails
            }

            return true;
        }

        private async Task SendDeactivationEmailAsync(string email, string firstName, string reason)
        {
            var subject = "Account Deactivated - Fundoo Notes";
            var body = $@"
                <html>
                <body>
                    <h2>Account Deactivated</h2>
                    <p>Hello {firstName},</p>
                    <p>Your Fundoo Notes account has been deactivated as requested.</p>
                    <p><strong>Reason:</strong> {reason}</p>
                    <p>If you wish to reactivate your account in the future, please contact our support team.</p>
                    <p>Thank you for using Fundoo Notes.</p>
                    <p>Best regards,<br>Fundoo Notes Team</p>
                </body>
                </html>";

            // Use the existing email service method or create a new one
            var emailService = _emailService as EmailService;
            if (emailService != null)
            {
                await emailService.SendEmailAsync(email, subject, body);
            }
        }
    }
}
