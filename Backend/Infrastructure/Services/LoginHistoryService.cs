using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Service for managing user login history
    /// </summary>
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly FundooNotesDbContext _context;
        private readonly ILogger<LoginHistoryService> _logger;

        public LoginHistoryService(FundooNotesDbContext context, ILogger<LoginHistoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RecordSuccessfulLoginAsync(int userId, string ipAddress, string userAgent, string? device = null)
        {
            try
            {
                // Login history recording is disabled - database table not implemented
                _logger.LogInformation("Login history recording disabled - Successful login for user: {UserId} from IP: {IpAddress}", userId, ipAddress);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording successful login for user: {UserId}", userId);
                // Don't throw - login history is not critical for the login process
            }
        }

        public async Task RecordFailedLoginAsync(int? userId, string email, string ipAddress, string userAgent, string failureReason, string? device = null)
        {
            try
            {
                // TEMPORARY: Disable login history recording until database table is created
                _logger.LogWarning("Login history recording disabled - Failed login for email: {Email} from IP: {IpAddress}, Reason: {Reason}",
                    email, ipAddress, failureReason);
                await Task.CompletedTask;

                /* TODO: Enable when LoginHistory table is created in database
                // If userId is not provided, try to find it by email
                if (userId == null)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    userId = user?.Id;
                }

                var loginHistory = new LoginHistory
                {
                    UserId = userId ?? 0, // Use 0 for unknown users
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Device = device ?? ExtractDeviceFromUserAgent(userAgent),
                    Location = "Unknown",
                    IsSuccessful = false,
                    FailureReason = failureReason,
                    LoginTime = DateTime.UtcNow
                };

                _context.LoginHistory.Add(loginHistory);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Failed login recorded for email: {Email} from IP: {IpAddress}, Reason: {Reason}",
                    email, ipAddress, failureReason);
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording failed login for email: {Email}", email);
                // Don't throw - login history is not critical for the login process
            }
        }

        public async Task RecordLogoutAsync(int userId, DateTime loginTime)
        {
            try
            {
                // TEMPORARY: Disable login history recording until database table is created
                _logger.LogInformation("Login history recording disabled - Logout for user: {UserId}", userId);
                await Task.CompletedTask;

                /* TODO: Enable when LoginHistory table is created in database
                // Find the most recent successful login record for this user around the login time
                var loginRecord = await _context.LoginHistory
                    .Where(lh => lh.UserId == userId &&
                                lh.IsSuccessful &&
                                lh.LogoutTime == null &&
                                Math.Abs((lh.LoginTime - loginTime).TotalMinutes) < 5) // Within 5 minutes
                    .OrderByDescending(lh => lh.LoginTime)
                    .FirstOrDefaultAsync();

                if (loginRecord != null)
                {
                    loginRecord.LogoutTime = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Logout recorded for user: {UserId}", userId);
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording logout for user: {UserId}", userId);
                // Don't throw - login history is not critical
            }
        }

        public async Task<IEnumerable<LoginHistory>> GetUserLoginHistoryAsync(int userId, int limit = 10)
        {
            try
            {
                // TEMPORARY: Return empty list until database table is created
                _logger.LogInformation("Login history recording disabled - Returning empty history for user: {UserId}", userId);
                await Task.CompletedTask;
                return new List<LoginHistory>();

                /* TODO: Enable when LoginHistory table is created in database
                return await _context.LoginHistory
                    .Where(lh => lh.UserId == userId)
                    .OrderByDescending(lh => lh.LoginTime)
                    .Take(limit)
                    .ToListAsync();
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login history for user: {UserId}", userId);
                return new List<LoginHistory>();
            }
        }

        public async Task<int> GetFailedLoginAttemptsAsync(string ipAddress, TimeSpan? timeWindow = null)
        {
            try
            {
                // TEMPORARY: Return 0 until database table is created
                _logger.LogInformation("Login history recording disabled - Returning 0 failed attempts for IP: {IpAddress}", ipAddress);
                await Task.CompletedTask;
                return 0;

                /* TODO: Enable when LoginHistory table is created in database
                var window = timeWindow ?? TimeSpan.FromHours(1);
                var cutoffTime = DateTime.UtcNow.Subtract(window);

                return await _context.LoginHistory
                    .Where(lh => lh.IpAddress == ipAddress &&
                                !lh.IsSuccessful &&
                                lh.LoginTime >= cutoffTime)
                    .CountAsync();
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting failed login attempts for IP: {IpAddress}", ipAddress);
                return 0;
            }
        }

        private string ExtractDeviceFromUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            // Simple device detection - in production, use a proper user agent parser
            if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
                return "Mobile";
            if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
                return "Tablet";
            if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                return "Windows";
            if (userAgent.Contains("Mac", StringComparison.OrdinalIgnoreCase))
                return "Mac";
            if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
                return "Linux";

            return "Desktop";
        }
    }
}
