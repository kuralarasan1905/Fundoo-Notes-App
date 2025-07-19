using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// Note history repository interface with history-specific operations
    /// </summary>
    public interface INoteHistoryRepository : IGenericRepository<NoteHistory>
    {
        Task<IEnumerable<NoteHistory>> GetByNoteIdAsync(int noteId, CancellationToken cancellationToken = default);
        Task<IEnumerable<NoteHistory>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task AddHistoryEntryAsync(int noteId, string action, string? oldValue = null, string? newValue = null, CancellationToken cancellationToken = default);
    }
}
