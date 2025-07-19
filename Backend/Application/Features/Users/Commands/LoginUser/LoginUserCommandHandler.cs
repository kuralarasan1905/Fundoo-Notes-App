using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;
using fundoo_notes.Application.Services;

namespace fundoo_notes.Application.Features.Users.Commands.LoginUser
{
    /// <summary>
    /// Handler for LoginUserCommand
    /// </summary>
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoginUserCommandHandler> _logger;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtTokenService jwtTokenService,
            ILoginHistoryService loginHistoryService,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<LoginUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _loginHistoryService = loginHistoryService;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing login attempt for email: {Email}", request.Email);

            // Get HTTP context for logging
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email: {Email}", request.Email);

                // Record failed login attempt
                await _loginHistoryService.RecordFailedLoginAsync(null, request.Email, ipAddress, userAgent, "User not found");

                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User account is inactive for email: {Email}", request.Email);

                // Record failed login attempt
                await _loginHistoryService.RecordFailedLoginAsync(user.Id, request.Email, ipAddress, userAgent, "Account inactive");

                throw new UnauthorizedAccessException("User account is inactive");
            }

            // Verify password
            var trimmedPassword = request.Password?.Trim();
            if (string.IsNullOrEmpty(trimmedPassword) || !_passwordService.VerifyPassword(trimmedPassword, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for email: {Email}. Password length: {PasswordLength}",
                    request.Email, request.Password?.Length ?? 0);

                // Record failed login attempt
                await _loginHistoryService.RecordFailedLoginAsync(user.Id, request.Email, ipAddress, userAgent, "Invalid password");

                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Update last login time
            await _userRepository.UpdateLastLoginAsync(user.Id, cancellationToken);

            // Record successful login in history
            await _loginHistoryService.RecordSuccessfulLoginAsync(user.Id, ipAddress, userAgent);

            // Set current user context for session-based access (for development/testing)
            try
            {
                await _currentUserService.SetCurrentUserAsync(user.Id);
                _logger.LogInformation("User context set in session/cookie for user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set user context in session/cookie, but login will continue with JWT only");
            }

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);
            var tokenExpiration = _jwtTokenService.GetTokenExpiration(token);

            var userDto = _mapper.Map<UserDto>(user);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = tokenExpiration,
                User = userDto
            };
        }
    }
}
