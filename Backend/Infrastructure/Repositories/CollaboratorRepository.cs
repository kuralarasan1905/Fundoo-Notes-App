using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// Collaborator repository implementation with collaborator-specific operations
    /// </summary>
    public class CollaboratorRepository : GenericRepository<Collaborator>, ICollaboratorRepository
    {
        public CollaboratorRepository(FundooNotesDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Collaborator>> GetByNoteIdAsync(int noteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.User)
                .Where(c => c.NoteId == noteId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Collaborator?> GetByNoteAndUserAsync(int noteId, int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.NoteId == noteId && c.UserId == userId, cancellationToken);
        }

        public async Task<bool> IsCollaboratorAsync(int noteId, int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(c => c.NoteId == noteId && c.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Collaborator>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Note)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
