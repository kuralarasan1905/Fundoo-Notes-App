using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// Note repository implementation with note-specific operations
    /// </summary>
    public class NoteRepository : GenericRepository<Note>, INoteRepository
    {
        public NoteRepository(FundooNotesDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Note>> GetUserNotesAsync(int userId, bool includeArchived = false, bool includeTrashed = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(n => n.UserId == userId);

            if (!includeArchived)
                query = query.Where(n => !n.IsArchived);

            if (!includeTrashed)
                query = query.Where(n => !n.IsTrashed);

            return await query
                .AsSplitQuery() // Optimize multiple includes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .AsNoTracking() // Read-only optimization for list queries
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetPinnedNotesAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && n.IsPinned && !n.IsArchived && !n.IsTrashed)
                .AsSplitQuery()
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .OrderByDescending(n => n.UpdatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetArchivedNotesAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && n.IsArchived && !n.IsTrashed)
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetTrashedNotesAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && n.IsTrashed)
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetNotesByLabelAsync(int userId, int labelId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && 
                           n.NoteLabels.Any(nl => nl.LabelId == labelId) && 
                           !n.IsTrashed)
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> SearchNotesAsync(int userId, string searchTerm, CancellationToken cancellationToken = default)
        {
            // Use database-level search with proper indexing
            return await _dbSet
                .Where(n => n.UserId == userId &&
                           !n.IsTrashed &&
                           (EF.Functions.Contains(n.Title, searchTerm) ||
                            EF.Functions.Contains(n.Content, searchTerm)))
                .AsSplitQuery() // Optimize multiple includes
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .AsNoTracking() // Read-only query optimization
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetNotesWithRemindersAsync(DateTime? beforeDateTime = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(n => n.ReminderDateTime.HasValue && !n.IsTrashed);

            if (beforeDateTime.HasValue)
                query = query.Where(n => n.ReminderDateTime <= beforeDateTime.Value);

            return await query
                .Include(n => n.User)
                .OrderBy(n => n.ReminderDateTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetSharedNotesAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.Collaborators.Any(c => c.UserId == userId && c.IsAccepted) && !n.IsTrashed)
                .Include(n => n.User)
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .Include(n => n.Collaborators)
                    .ThenInclude(c => c.User)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsNoteOwnerAsync(int noteId, int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(n => n.Id == noteId && n.UserId == userId, cancellationToken);
        }

        public async Task<bool> HasNoteAccessAsync(int noteId, int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(n => n.Id == noteId &&
                (n.UserId == userId || n.Collaborators.Any(c => c.UserId == userId && c.IsAccepted)),
                cancellationToken);
        }

        public async Task PinNoteAsync(int noteId, bool isPinned, CancellationToken cancellationToken = default)
        {
            var note = await GetByIdAsync(noteId, cancellationToken);
            if (note == null)
            {
                throw new ArgumentException($"Note with ID {noteId} not found", nameof(noteId));
            }

            // Only update if the pin status is actually changing
            if (note.IsPinned != isPinned)
            {
                note.IsPinned = isPinned;
                note.UpdatedAt = DateTime.UtcNow;

                await UpdateAsync(note, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }
        }

        public async Task ArchiveNoteAsync(int noteId, bool isArchived, CancellationToken cancellationToken = default)
        {
            var note = await GetByIdAsync(noteId, cancellationToken);
            if (note != null)
            {
                note.IsArchived = isArchived;
                if (isArchived)
                    note.IsPinned = false; // Unpin when archiving
                await UpdateAsync(note, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }
        }

        public async Task TrashNoteAsync(int noteId, bool isTrashed, CancellationToken cancellationToken = default)
        {
            var note = await GetByIdAsync(noteId, cancellationToken);
            if (note != null)
            {
                note.IsTrashed = isTrashed;
                if (isTrashed)
                {
                    note.IsPinned = false; // Unpin when trashing
                    note.IsArchived = false; // Unarchive when trashing
                }
                await UpdateAsync(note, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetUserNotesCountAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsTrashed, cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetNotesByIdsAsync(IEnumerable<int> noteIds, int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => noteIds.Contains(n.Id) && n.UserId == userId)
                .AsSplitQuery()
                .Include(n => n.NoteLabels)
                    .ThenInclude(nl => nl.Label)
                .ToListAsync(cancellationToken);
        }

        public async Task BulkUpdateAsync(IEnumerable<Note> notes, CancellationToken cancellationToken = default)
        {
            _context.UpdateRange(notes);
        }

        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}
