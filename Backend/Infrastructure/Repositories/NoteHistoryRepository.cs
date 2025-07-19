using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// Note history repository implementation with history-specific operations
    /// </summary>
    public class NoteHistoryRepository : GenericRepository<NoteHistory>, INoteHistoryRepository
    {
        public NoteHistoryRepository(FundooNotesDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NoteHistory>> GetByNoteIdAsync(int noteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(h => h.NoteId == noteId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<NoteHistory>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(h => h.Note)
                .Where(h => h.Note.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddHistoryEntryAsync(int noteId, string action, string? oldValue = null, string? newValue = null, CancellationToken cancellationToken = default)
        {
            var historyEntry = new NoteHistory
            {
                NoteId = noteId,
                UserId = 0, // This should be set by the caller
                Action = action,
                ChangeDetails = $"Old: {oldValue}, New: {newValue}",
                ActionDateTime = DateTime.UtcNow
            };

            await AddAsync(historyEntry);
        }
    }
}
