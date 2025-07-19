using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;

namespace fundoo_notes.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Handler for ChangePasswordCommand
    /// </summary>
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing password change for user: {UserId}", request.UserId);

            // Validate passwords match
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("Password change failed: New passwords do not match for user: {UserId}", request.UserId);
                throw new ArgumentException("New passwords do not match");
            }

            // Get user by ID
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Password change failed: User not found: {UserId}", request.UserId);
                throw new ArgumentException("User not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Password change failed: User account is inactive: {UserId}", request.UserId);
                throw new ArgumentException("User account is inactive");
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Password change failed: Invalid current password for user: {UserId}", request.UserId);
                throw new ArgumentException("Current password is incorrect");
            }

            // Update password
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);

            return true;
        }
    }
}
