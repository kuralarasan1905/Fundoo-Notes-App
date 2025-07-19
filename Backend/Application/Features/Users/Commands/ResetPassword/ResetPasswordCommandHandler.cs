using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;

namespace fundoo_notes.Application.Features.Users.Commands.ResetPassword
{
    /// <summary>
    /// Handler for ResetPasswordCommand
    /// </summary>
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ILogger<ResetPasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing password reset with token");

            // Validate passwords match
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("Password reset failed: Passwords do not match");
                throw new ArgumentException("Passwords do not match");
            }

            // Get user by reset token
            var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Password reset failed: Invalid token");
                throw new ArgumentException("Invalid or expired reset token");
            }

            // Check if token is expired
            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Password reset failed: Token expired for user: {UserId}", user.Id);
                throw new ArgumentException("Reset token has expired");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Password reset failed: User account is inactive: {UserId}", user.Id);
                throw new ArgumentException("User account is inactive");
            }

            // Update password and clear reset token
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);

            return true;
        }
    }
}
