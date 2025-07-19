using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// Note repository interface with note-specific operations
    /// </summary>
    public interface INoteRepository : IGenericRepository<Note>
    {
        Task<IEnumerable<Note>> GetUserNotesAsync(int userId, bool includeArchived = false, bool includeTrashed = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> GetPinnedNotesAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> GetArchivedNotesAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> GetTrashedNotesAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> GetNotesByLabelAsync(int userId, int labelId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> SearchNotesAsync(int userId, string searchTerm, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> GetNotesWithRemindersAsync(DateTime? beforeDateTime = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> GetSharedNotesAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> IsNoteOwnerAsync(int noteId, int userId, CancellationToken cancellationToken = default);
        Task<bool> HasNoteAccessAsync(int noteId, int userId, CancellationToken cancellationToken = default);
        Task PinNoteAsync(int noteId, bool isPinned, CancellationToken cancellationToken = default);
        Task ArchiveNoteAsync(int noteId, bool isArchived, CancellationToken cancellationToken = default);
        Task TrashNoteAsync(int noteId, bool isTrashed, CancellationToken cancellationToken = default);
        Task<int> GetUserNotesCountAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Note>> GetNotesByIdsAsync(IEnumerable<int> noteIds, int userId, CancellationToken cancellationToken = default);
        Task BulkUpdateAsync(IEnumerable<Note> notes, CancellationToken cancellationToken = default);
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
