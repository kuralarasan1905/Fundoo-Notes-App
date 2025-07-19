using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// User repository implementation with user-specific operations
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(FundooNotesDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(
                u => u.EmailVerificationToken == token && 
                     u.EmailVerificationTokenExpiry > DateTime.UtcNow, 
                cancellationToken);
        }

        public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(
                u => u.PasswordResetToken == token && 
                     u.PasswordResetTokenExpiry > DateTime.UtcNow, 
                cancellationToken);
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10, CancellationToken cancellationToken = default)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            
            return await _dbSet
                .Where(u => u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                           u.LastName.ToLower().Contains(lowerSearchTerm) ||
                           u.Email.ToLower().Contains(lowerSearchTerm))
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await UpdateAsync(user, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await GetByEmailAsync(email, cancellationToken);
            if (user == null || !user.IsActive)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
