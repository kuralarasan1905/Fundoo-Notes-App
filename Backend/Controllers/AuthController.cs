using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Users.Commands.RegisterUser;
using fundoo_notes.Application.Features.Users.Commands.LoginUser;
using fundoo_notes.Application.Features.Users.Commands.ForgotPassword;
using fundoo_notes.Application.Features.Users.Commands.ResetPassword;
using fundoo_notes.Application.Features.Users.Commands.VerifyEmail;
using fundoo_notes.Application.Features.Users.Commands.ResendVerification;
using fundoo_notes.Application.Services;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Authentication controller for user registration and login
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(IMediator mediator, ILogger<AuthController> logger, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        // DEVELOPMENT ENDPOINTS COMMENTED OUT FOR PRODUCTION
        /*
        /// <summary>
        /// TEMPORARY: Generate test JWT token for frontend development
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <returns>Test JWT token</returns>
        [HttpGet("test-token")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult<object> GetTestToken()
        {
            // This creates a test token for your user (ID: 2)
            var testUser = new
            {
                Id = 2,
                FirstName = "kural",
                LastName = "Rrrrr",
                Email = "kuralarasan1905@gmail.com"
            };

            // Generate token using your JWT service
            var jwtService = HttpContext.RequestServices.GetRequiredService<fundoo_notes.Infrastructure.Services.IJwtTokenService>();

            // Create a User entity for token generation
            var user = new fundoo_notes.Domain.Entities.User
            {
                Id = 2,
                FirstName = "kural",
                LastName = "Rrrrr",
                Email = "kuralarasan1905@gmail.com",
                IsActive = true
            };

            var token = jwtService.GenerateToken(user);
            var expiration = jwtService.GetTokenExpiration(token);

            return Ok(new
            {
                token = token,
                expiresAt = expiration,
                user = testUser,
                message = "WARNING: This is a TEST token for development only!"
            });
        }

        /// <summary>
        /// TEMPORARY: Create/Reset test user for development
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <returns>Test user creation result</returns>
        [HttpPost("create-test-user")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> CreateTestUser()
        {
            try
            {
                var userRepository = HttpContext.RequestServices.GetRequiredService<fundoo_notes.Domain.Interfaces.IUserRepository>();
                var passwordService = HttpContext.RequestServices.GetRequiredService<fundoo_notes.Infrastructure.Services.IPasswordService>();

                var testEmail = "kuralarasan1905@gmail.com";
                var testPassword = "kural1905"; // Simple password for testing

                // Check if user already exists
                var existingUser = await userRepository.GetByEmailAsync(testEmail);
                if (existingUser != null)
                {
                    // Update existing user's password
                    existingUser.PasswordHash = passwordService.HashPassword(testPassword);
                    existingUser.IsActive = true;
                    existingUser.IsEmailVerified = true;
                    await userRepository.UpdateAsync(existingUser);
                    await userRepository.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Test user updated successfully!",
                        email = testEmail,
                        password = testPassword,
                        userId = existingUser.Id,
                        note = "You can now login with these credentials"
                    });
                }
                else
                {
                    // Create new test user
                    var newUser = new fundoo_notes.Domain.Entities.User
                    {
                        FirstName = "kural",
                        LastName = "Rrrrr",
                        Email = testEmail,
                        PasswordHash = passwordService.HashPassword(testPassword),
                        IsActive = true,
                        IsEmailVerified = true,
                        EmailVerificationToken = null,
                        EmailVerificationTokenExpiry = null
                    };

                    await userRepository.AddAsync(newUser);
                    await userRepository.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Test user created successfully!",
                        email = testEmail,
                        password = testPassword,
                        userId = newUser.Id,
                        note = "You can now login with these credentials"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test user");
                return BadRequest($"Failed to create test user: {ex.Message}");
            }
        }

        /// <summary>
        /// TEMPORARY: Debug user login - check if user exists and password matches
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="password">Password to verify</param>
        /// <returns>Debug information</returns>
        [HttpPost("debug-login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> DebugLogin([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                var userRepository = HttpContext.RequestServices.GetRequiredService<fundoo_notes.Domain.Interfaces.IUserRepository>();
                var passwordService = HttpContext.RequestServices.GetRequiredService<fundoo_notes.Infrastructure.Services.IPasswordService>();

                // Check if user exists
                var user = await userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    return Ok(new
                    {
                        status = "USER NOT FOUND",
                        email = email,
                        message = "No user found with this email address",
                        suggestion = "Use the create-test-user endpoint first"
                    });
                }

                // Check password
                var passwordMatch = passwordService.VerifyPassword(password, user.PasswordHash);

                return Ok(new
                {
                    status = passwordMatch ? "LOGIN SHOULD WORK" : "PASSWORD MISMATCH",
                    email = email,
                    userId = user.Id,
                    isActive = user.IsActive,
                    isEmailVerified = user.IsEmailVerified,
                    passwordMatch = passwordMatch,
                    passwordLength = password?.Length ?? 0,
                    hashPrefix = user.PasswordHash?.Length > 10 ? user.PasswordHash.Substring(0, 10) : user.PasswordHash,
                    message = passwordMatch ? "Login should work with these credentials" : "Password does not match stored hash"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = "ERROR",
                    error = ex.Message,
                    message = "An error occurred during debug check"
                });
            }
        }

        /// <summary>
        /// TEMPORARY: Set user ID in session for testing without JWT
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <param name="userId">User ID to set in session</param>
        /// <returns>Success message</returns>
        [HttpPost("set-session-user/{userId:int}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult<object> SetSessionUser(int userId)
        {
            // Set user ID in session
            HttpContext.Session.SetInt32("UserId", userId);

            // Also set in cookie for persistence
            Response.Cookies.Append("UserId", userId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                message = $"User ID {userId} set in session and cookie",
                userId = userId,
                sessionId = HttpContext.Session.Id,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// TEMPORARY: Clear user session for testing
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("clear-session")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult<object> ClearSession()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserId");

            return Ok(new
            {
                message = "Session and cookies cleared",
                timestamp = DateTime.UtcNow
            });
        }
        */

        /// <summary>
        /// PROFESSIONAL: Set current user context by user ID for development/testing
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <param name="userId">User ID to set as current user</param>
        /// <returns>Success message</returns>
        [HttpPost("set-user-context/{userId:int}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> SetUserContext(int userId)
        {
            try
            {
                await _currentUserService.SetCurrentUserAsync(userId);
                var user = await _currentUserService.GetCurrentUserAsync();

                return Ok(new
                {
                    message = $"Current user context set successfully",
                    userId = userId,
                    userEmail = user?.Email,
                    userName = $"{user?.FirstName} {user?.LastName}".Trim(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user context for userId: {UserId}", userId);
                return BadRequest(new { message = "Error setting user context", error = ex.Message });
            }
        }

        /// <summary>
        /// PROFESSIONAL: Set current user context by email for development/testing
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <param name="email">User email to set as current user</param>
        /// <returns>Success message</returns>
        [HttpPost("set-user-by-email/{email}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> SetUserByEmail(string email)
        {
            try
            {
                await _currentUserService.SetCurrentUserByEmailAsync(email);
                var user = await _currentUserService.GetCurrentUserAsync();

                return Ok(new
                {
                    message = $"Current user context set successfully",
                    userId = user?.Id,
                    userEmail = user?.Email,
                    userName = $"{user?.FirstName} {user?.LastName}".Trim(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// PROFESSIONAL: Clear current user context for development/testing
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("clear-user-context")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> ClearUserContext()
        {
            await _currentUserService.ClearCurrentUserAsync();

            return Ok(new
            {
                message = "Current user context cleared successfully",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// PROFESSIONAL: Get current user context for development/testing
        /// TODO: Remove this endpoint before production
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("current-user-context")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetCurrentUserContext()
        {
            if (!_currentUserService.HasCurrentUser())
            {
                return NotFound(new
                {
                    message = "No current user context found",
                    suggestion = "Call /api/auth/set-user-context/{userId} or /api/auth/set-user-by-email/{email} first",
                    timestamp = DateTime.UtcNow
                });
            }

            var user = await _currentUserService.GetCurrentUserAsync();
            return Ok(new
            {
                message = "Current user context found",
                userId = user?.Id,
                userEmail = user?.Email,
                userName = $"{user?.FirstName} {user?.LastName}".Trim(),
                isActive = user?.IsActive,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] UserRegistrationDto request)
        {
            try
            {
                var command = new RegisterUserCommand
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = request.Password,
                    ConfirmPassword = request.ConfirmPassword
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(Register), result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for email: {Email}", request.Email);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for email: {Email}", request.Email);
                return BadRequest("Registration failed. Please try again.");
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="request">User login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] UserLoginDto request)
        {
            try
            {
                var command = new LoginUserCommand
                {
                    Email = request.Email,
                    Password = request.Password
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed for email: {Email}", request.Email);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
                return BadRequest("Login failed. Please try again.");
            }
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        /// <param name="request">Email for password reset</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                var command = new ForgotPasswordCommand
                {
                    Email = request.Email
                };

                var result = await _mediator.Send(command);

                return Ok(new {
                    message = "If an account with that email exists, a password reset link has been sent.",
                    success = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request for email: {Email}", request.Email);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        /// <param name="request">Password reset data</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                var command = new ResetPasswordCommand
                {
                    Token = request.Token,
                    NewPassword = request.NewPassword,
                    ConfirmNewPassword = request.ConfirmNewPassword
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new { message = "Password reset successfully" });
                }

                return BadRequest("Failed to reset password");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Password reset failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, "An error occurred while resetting password");
            }
        }

        /// <summary>
        /// Verify email address
        /// </summary>
        /// <param name="request">Email verification data</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            try
            {
                var command = new VerifyEmailCommand
                {
                    Email = request.Email,
                    Token = request.Token
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new { message = "Email verified successfully" });
                }

                return BadRequest("Email verification failed. Token may be invalid or expired.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for email: {Email}", request.Email);
                return StatusCode(500, "An error occurred while verifying email");
            }
        }

        /// <summary>
        /// Resend email verification
        /// </summary>
        /// <param name="request">Email for verification resend</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> ResendVerification([FromBody] ResendVerificationDto request)
        {
            try
            {
                var command = new ResendVerificationCommand
                {
                    Email = request.Email
                };

                var result = await _mediator.Send(command);

                return Ok(new {
                    message = "If an account with that email exists and is not verified, a verification email has been sent.",
                    success = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during verification resend for email: {Email}", request.Email);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
