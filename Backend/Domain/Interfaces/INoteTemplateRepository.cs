using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// Note template repository interface with template-specific operations
    /// </summary>
    public interface INoteTemplateRepository : IGenericRepository<NoteTemplate>
    {
        Task<IEnumerable<NoteTemplate>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<NoteTemplate>> GetPublicTemplatesAsync(CancellationToken cancellationToken = default);
        Task<NoteTemplate?> GetByIdAndUserAsync(int templateId, int userId, CancellationToken cancellationToken = default);
    }
}
