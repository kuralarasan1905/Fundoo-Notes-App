using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// User repository interface with user-specific operations
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10, CancellationToken cancellationToken = default);
        Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> ValidateUserCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
    }
}
