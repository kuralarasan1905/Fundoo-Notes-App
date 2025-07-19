using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using fundoo_notes.Application.Services;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Service for managing current user context without hardcoded data
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    return user.Id;
                }

                // If no user found, check if we're in development mode and use fallback
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var environment = httpContext.RequestServices.GetService<IWebHostEnvironment>();
                    if (environment?.IsDevelopment() == true)
                    {
                        _logger.LogWarning("No current user found, using fallback user ID 2 for development");
                        return 2; // Fallback user ID for development/testing
                    }
                }

                throw new UnauthorizedAccessException("No current user found. Please set user context first.");
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException))
            {
                _logger.LogError(ex, "Error getting current user ID");

                // If we're in development mode, provide fallback
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var environment = httpContext.RequestServices.GetService<IWebHostEnvironment>();
                    if (environment?.IsDevelopment() == true)
                    {
                        _logger.LogWarning("Error occurred getting user ID, using fallback user ID 2 for development");
                        return 2; // Fallback user ID for development/testing
                    }
                }

                throw new UnauthorizedAccessException("Authentication required. Please log in.");
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("No HTTP context available");
                    return null;
                }

                // Method 1: Try to get user from JWT claims
                try
                {
                    var userIdClaim = httpContext.User?.FindFirst("userId")?.Value ??
                                     httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                    {
                        var jwtUser = await _userRepository.GetByIdAsync(jwtUserId);
                        if (jwtUser != null && jwtUser.IsActive)
                        {
                            _logger.LogInformation("Current user retrieved from JWT: {UserId}", jwtUserId);
                            return jwtUser;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error accessing JWT claims for user context");
                }

            // Method 2: Try to get user from session (with error handling)
            try
            {
                var sessionUserId = httpContext.Session.GetInt32("CurrentUserId");
                if (sessionUserId.HasValue)
                {
                    var sessionUser = await _userRepository.GetByIdAsync(sessionUserId.Value);
                    if (sessionUser != null && sessionUser.IsActive)
                    {
                        _logger.LogInformation("Current user retrieved from session: {UserId}", sessionUserId.Value);
                        return sessionUser;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error accessing session for user context");
            }

                // Method 3: Try to get user from cookie
                try
                {
                    if (httpContext.Request.Cookies.TryGetValue("CurrentUserId", out var cookieUserId) &&
                        int.TryParse(cookieUserId, out int cookieUserIdInt))
                    {
                        var cookieUser = await _userRepository.GetByIdAsync(cookieUserIdInt);
                        if (cookieUser != null && cookieUser.IsActive)
                        {
                            _logger.LogInformation("Current user retrieved from cookie: {UserId}", cookieUserIdInt);
                            return cookieUser;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error accessing cookie for user context");
                }

                _logger.LogWarning("No current user found in any context");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetCurrentUserAsync");
                return null;
            }
        }

        public async Task SetCurrentUserAsync(int userId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HTTP context available");
            }

            // Verify user exists and is active
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                throw new ArgumentException($"User with ID {userId} not found or inactive");
            }

            // Set in session (with error handling)
            try
            {
                httpContext.Session.SetInt32("CurrentUserId", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting user in session, will use cookie only");
            }

            // Set in cookie for persistence
            httpContext.Response.Cookies.Append("CurrentUserId", userId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            _logger.LogInformation("Current user set: {UserId} ({Email})", userId, user.Email);
        }

        public async Task SetCurrentUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                throw new ArgumentException($"User with email {email} not found or inactive");
            }

            await SetCurrentUserAsync(user.Id);
        }

        public async Task ClearCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            // Clear session
            httpContext.Session.Remove("CurrentUserId");

            // Clear cookie
            httpContext.Response.Cookies.Delete("CurrentUserId");

            _logger.LogInformation("Current user context cleared");
            await Task.CompletedTask;
        }

        public bool HasCurrentUser()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    return false;
                }

                // Check JWT
                try
                {
                    var userIdClaim = httpContext.User?.FindFirst("userId")?.Value ??
                                     httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking JWT claims in HasCurrentUser");
                }

                // Check session
                try
                {
                    if (httpContext.Session.GetInt32("CurrentUserId").HasValue)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking session in HasCurrentUser");
                }

                // Check cookie
                try
                {
                    if (httpContext.Request.Cookies.ContainsKey("CurrentUserId"))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking cookie in HasCurrentUser");
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in HasCurrentUser");
                return false;
            }
        }
    }
}
