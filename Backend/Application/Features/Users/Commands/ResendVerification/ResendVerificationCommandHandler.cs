using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;

namespace fundoo_notes.Application.Features.Users.Commands.ResendVerification
{
    /// <summary>
    /// Handler for ResendVerificationCommand
    /// </summary>
    public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendVerificationCommandHandler> _logger;

        public ResendVerificationCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IEmailService emailService,
            ILogger<ResendVerificationCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing resend verification request for email: {Email}", request.Email);

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Resend verification requested for non-existent email: {Email}", request.Email);
                // Return true to prevent email enumeration attacks
                return true;
            }

            // Check if email is already verified
            if (user.IsEmailVerified)
            {
                _logger.LogInformation("Email already verified for user: {UserId}", user.Id);
                return true;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Resend verification requested for inactive user: {Email}", request.Email);
                return false;
            }

            // Generate new verification token
            user.EmailVerificationToken = _passwordService.GenerateRandomToken();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

            // Save user with new verification token
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New verification token generated for user: {UserId}", user.Id);

            // Send verification email
            try
            {
                await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken);
                _logger.LogInformation("Verification email resent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend verification email to: {Email}", user.Email);
                // Don't fail the operation if email sending fails
            }

            return true;
        }
    }
}
