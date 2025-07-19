using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;

namespace fundoo_notes.Application.Features.Users.Commands.ForgotPassword
{
    /// <summary>
    /// Handler for ForgotPasswordCommand
    /// </summary>
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IEmailService emailService,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing password reset request for email: {Email}", request.Email);

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                // Return true to prevent email enumeration attacks
                return true;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Password reset requested for inactive user: {Email}", request.Email);
                return false;
            }

            // Generate password reset token
            user.PasswordResetToken = _passwordService.GenerateRandomToken();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

            // Save user with reset token
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);

            // Send password reset email
            try
            {
                await _emailService.SendPasswordResetAsync(user.Email, user.PasswordResetToken);
                _logger.LogInformation("Password reset email sent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to: {Email}", user.Email);
                // Don't fail the operation if email sending fails
            }

            return true;
        }
    }
}
