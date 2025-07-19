using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// Collaborator repository interface with collaborator-specific operations
    /// </summary>
    public interface ICollaboratorRepository : IGenericRepository<Collaborator>
    {
        Task<IEnumerable<Collaborator>> GetByNoteIdAsync(int noteId, CancellationToken cancellationToken = default);
        Task<Collaborator?> GetByNoteAndUserAsync(int noteId, int userId, CancellationToken cancellationToken = default);
        Task<bool> IsCollaboratorAsync(int noteId, int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Collaborator>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    }
}
