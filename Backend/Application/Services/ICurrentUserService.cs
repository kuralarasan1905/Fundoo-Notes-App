using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Application.Services
{
    /// <summary>
    /// Service interface for managing current user context
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Get the current user ID
        /// </summary>
        /// <returns>Current user ID</returns>
        Task<int> GetCurrentUserIdAsync();

        /// <summary>
        /// Get the current user entity
        /// </summary>
        /// <returns>Current user entity</returns>
        Task<User?> GetCurrentUserAsync();

        /// <summary>
        /// Set the current user context (for development/testing)
        /// </summary>
        /// <param name="userId">User ID to set</param>
        Task SetCurrentUserAsync(int userId);

        /// <summary>
        /// Set the current user context by email (for development/testing)
        /// </summary>
        /// <param name="email">User email</param>
        Task SetCurrentUserByEmailAsync(string email);

        /// <summary>
        /// Clear the current user context
        /// </summary>
        Task ClearCurrentUserAsync();

        /// <summary>
        /// Check if a user is currently set
        /// </summary>
        /// <returns>True if user is set</returns>
        bool HasCurrentUser();
    }
}
