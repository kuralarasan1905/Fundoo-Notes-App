using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Users.Commands.VerifyEmail
{
    /// <summary>
    /// Handler for VerifyEmailCommand
    /// </summary>
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<VerifyEmailCommandHandler> _logger;

        public VerifyEmailCommandHandler(
            IUserRepository userRepository,
            ILogger<VerifyEmailCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing email verification for: {Email}", request.Email);

            // Get user by email and verification token
            var user = await _userRepository.GetByEmailVerificationTokenAsync(request.Token, cancellationToken);
            if (user == null || user.Email != request.Email)
            {
                _logger.LogWarning("Email verification failed: Invalid token or email mismatch");
                return false;
            }

            // Check if token is expired
            if (user.EmailVerificationTokenExpiry == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Email verification failed: Token expired for user: {UserId}", user.Id);
                return false;
            }

            // Check if email is already verified
            if (user.IsEmailVerified)
            {
                _logger.LogInformation("Email already verified for user: {UserId}", user.Id);
                return true;
            }

            // Verify email and clear verification token
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);

            return true;
        }
    }
}
