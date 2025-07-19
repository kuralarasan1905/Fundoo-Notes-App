namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Service interface for managing JWT token blacklist
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// Add a token to the blacklist
        /// </summary>
        /// <param name="token">JWT token to blacklist</param>
        /// <param name="expiration">Token expiration time</param>
        /// <returns>Task</returns>
        Task BlacklistTokenAsync(string token, DateTime expiration);

        /// <summary>
        /// Check if a token is blacklisted
        /// </summary>
        /// <param name="token">JWT token to check</param>
        /// <returns>True if token is blacklisted</returns>
        Task<bool> IsTokenBlacklistedAsync(string token);

        /// <summary>
        /// Clean up expired tokens from blacklist
        /// </summary>
        /// <returns>Number of tokens cleaned up</returns>
        Task<int> CleanupExpiredTokensAsync();

        /// <summary>
        /// Blacklist all tokens for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Task</returns>
        Task BlacklistUserTokensAsync(int userId);
    }
}
