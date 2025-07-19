using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Service interface for managing user login history
    /// </summary>
    public interface ILoginHistoryService
    {
        /// <summary>
        /// Record a successful login
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="ipAddress">IP address</param>
        /// <param name="userAgent">User agent string</param>
        /// <param name="device">Device information</param>
        /// <returns>Task</returns>
        Task RecordSuccessfulLoginAsync(int userId, string ipAddress, string userAgent, string? device = null);

        /// <summary>
        /// Record a failed login attempt
        /// </summary>
        /// <param name="userId">User ID (if known)</param>
        /// <param name="email">Email address used in login attempt</param>
        /// <param name="ipAddress">IP address</param>
        /// <param name="userAgent">User agent string</param>
        /// <param name="failureReason">Reason for failure</param>
        /// <param name="device">Device information</param>
        /// <returns>Task</returns>
        Task RecordFailedLoginAsync(int? userId, string email, string ipAddress, string userAgent, string failureReason, string? device = null);

        /// <summary>
        /// Record logout time
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="loginTime">Original login time to match</param>
        /// <returns>Task</returns>
        Task RecordLogoutAsync(int userId, DateTime loginTime);

        /// <summary>
        /// Get recent login history for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Number of records to return</param>
        /// <returns>List of login history records</returns>
        Task<IEnumerable<LoginHistory>> GetUserLoginHistoryAsync(int userId, int limit = 10);

        /// <summary>
        /// Get failed login attempts for security monitoring
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <param name="timeWindow">Time window to check (default: last hour)</param>
        /// <returns>Number of failed attempts</returns>
        Task<int> GetFailedLoginAttemptsAsync(string ipAddress, TimeSpan? timeWindow = null);
    }
}
