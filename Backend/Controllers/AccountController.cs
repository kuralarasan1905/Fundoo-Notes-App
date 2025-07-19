using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Users.Commands.UpdateProfile;
using fundoo_notes.Application.Features.Users.Commands.ChangePassword;
using fundoo_notes.Application.Features.Users.Commands.DeactivateAccount;
using fundoo_notes.Application.Features.Users.Queries.GetUserProfile;
using fundoo_notes.Application.Services;
using fundoo_notes.Infrastructure.Services;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Account management controller for user profile and account operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccountController> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILoginHistoryService _loginHistoryService;

        public AccountController(
            IMediator mediator,
            ILogger<AccountController> logger,
            ICurrentUserService currentUserService,
            ITokenBlacklistService tokenBlacklistService,
            IJwtTokenService jwtTokenService,
            ILoginHistoryService loginHistoryService)
        {
            _mediator = mediator;
            _logger = logger;
            _currentUserService = currentUserService;
            _tokenBlacklistService = tokenBlacklistService;
            _jwtTokenService = jwtTokenService;
            _loginHistoryService = loginHistoryService;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>User profile information</returns>
        [HttpGet("profile")]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();
                var query = new GetUserProfileQuery { UserId = userId };
                var profile = await _mediator.Send(query);

                return Ok(profile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Get profile failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Get profile failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "An error occurred while getting profile");
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="request">Profile update data</param>
        /// <returns>Updated user profile</returns>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();
                var command = new UpdateProfileCommand
                {
                    UserId = userId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Bio = request.Bio
                };

                var updatedProfile = await _mediator.Send(command);
                return Ok(updatedProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Update profile failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Update profile failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, "An error occurred while updating profile");
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change data</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();
                var command = new ChangePasswordCommand
                {
                    UserId = userId,
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword,
                    ConfirmNewPassword = request.ConfirmNewPassword
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new { message = "Password changed successfully" });
                }

                return BadRequest("Failed to change password");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Change password failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Change password failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "An error occurred while changing password");
            }
        }

        /// <summary>
        /// Get account settings
        /// </summary>
        /// <returns>Account settings</returns>
        [HttpGet("settings")]
        [ProducesResponseType(typeof(AccountSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AccountSettingsDto>> GetSettings()
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();

                // For now, return default settings
                // In a real application, you would store these in the database
                var settings = new AccountSettingsDto
                {
                    EmailNotifications = true,
                    PushNotifications = true,
                    Theme = "light",
                    Language = "en",
                    TimeZone = "UTC"
                };

                return Ok(settings);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Get settings failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account settings");
                return StatusCode(500, "An error occurred while getting settings");
            }
        }

        /// <summary>
        /// Update account settings
        /// </summary>
        /// <param name="settings">Account settings to update</param>
        /// <returns>Updated settings</returns>
        [HttpPut("settings")]
        [ProducesResponseType(typeof(AccountSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AccountSettingsDto>> UpdateSettings([FromBody] AccountSettingsDto settings)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();

                // For now, just return the settings as-is
                // In a real application, you would save these to the database
                _logger.LogInformation("Settings updated for user: {UserId}", userId);

                return Ok(settings);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Update settings failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account settings");
                return StatusCode(500, "An error occurred while updating settings");
            }
        }

        /// <summary>
        /// Deactivate user account
        /// </summary>
        /// <param name="request">Deactivation request with password confirmation</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("deactivate")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> DeactivateAccount([FromBody] DeactivateAccountDto request)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();
                var command = new DeactivateAccountCommand
                {
                    UserId = userId,
                    Password = request.Password,
                    Reason = request.Reason
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new {
                        message = "Account deactivated successfully. You have been logged out.",
                        timestamp = DateTime.UtcNow
                    });
                }

                return BadRequest("Failed to deactivate account");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Deactivate account failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Account deactivation failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating account");
                return StatusCode(500, "An error occurred while deactivating account");
            }
        }

        /// <summary>
        /// Logout user (clear session and invalidate token)
        /// </summary>
        /// <returns>Success confirmation</returns>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> Logout()
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();

                // Get the current JWT token from the Authorization header
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader["Bearer ".Length..].Trim();

                    // Get token expiration
                    var expiration = _jwtTokenService.GetTokenExpiration(token);

                    // Add token to blacklist
                    await _tokenBlacklistService.BlacklistTokenAsync(token, expiration);

                    _logger.LogInformation("JWT token blacklisted for user: {UserId}", userId);
                }

                // Clear current user context
                await _currentUserService.ClearCurrentUserAsync();

                _logger.LogInformation("User logged out successfully: {UserId}", userId);

                return Ok(new {
                    message = "Logged out successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred during logout");
            }
        }

        /// <summary>
        /// Logout from all devices (invalidate all user tokens)
        /// </summary>
        /// <returns>Success confirmation</returns>
        [HttpPost("logout-all")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> LogoutAll()
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();

                // Blacklist all tokens for this user
                await _tokenBlacklistService.BlacklistUserTokensAsync(userId);

                // Clear current user context
                await _currentUserService.ClearCurrentUserAsync();

                _logger.LogInformation("User logged out from all devices: {UserId}", userId);

                return Ok(new {
                    message = "Logged out from all devices successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Logout all failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout all");
                return StatusCode(500, "An error occurred during logout");
            }
        }

        /// <summary>
        /// Get user login history
        /// </summary>
        /// <param name="limit">Number of records to return (default: 10, max: 50)</param>
        /// <returns>List of login history records</returns>
        [HttpGet("login-history")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<object>>> GetLoginHistory([FromQuery] int limit = 10)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();

                // Limit the number of records to prevent abuse
                limit = Math.Min(Math.Max(limit, 1), 50);

                var loginHistory = await _loginHistoryService.GetUserLoginHistoryAsync(userId, limit);

                var result = loginHistory.Select(lh => new
                {
                    id = lh.Id,
                    ipAddress = lh.IpAddress,
                    device = lh.Device,
                    location = lh.Location,
                    isSuccessful = lh.IsSuccessful,
                    failureReason = lh.FailureReason,
                    loginTime = lh.LoginTime,
                    logoutTime = lh.LogoutTime,
                    userAgent = lh.UserAgent?.Length > 100 ? lh.UserAgent.Substring(0, 100) + "..." : lh.UserAgent
                });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Get login history failed: User not authenticated");
                return Unauthorized("User not authenticated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login history");
                return StatusCode(500, "An error occurred while getting login history");
            }
        }

        /// <summary>
        /// TEMPORARY: Test endpoint to check current user context
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <returns>Current user context information</returns>
        [HttpGet("test-user-context")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> TestUserContext()
        {
            try
            {
                var hasUser = _currentUserService.HasCurrentUser();
                if (!hasUser)
                {
                    return Ok(new
                    {
                        hasCurrentUser = false,
                        message = "No current user context found",
                        suggestion = "Try logging in first or call /api/auth/set-user-context/{userId}",
                        timestamp = DateTime.UtcNow
                    });
                }

                var user = await _currentUserService.GetCurrentUserAsync();
                var userId = await _currentUserService.GetCurrentUserIdAsync();

                return Ok(new
                {
                    hasCurrentUser = true,
                    userId = userId,
                    userEmail = user?.Email,
                    userName = user?.FullName,
                    isActive = user?.IsActive,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    hasCurrentUser = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
