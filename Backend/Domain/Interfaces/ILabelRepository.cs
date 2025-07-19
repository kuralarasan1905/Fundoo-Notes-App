using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// Label repository interface with label-specific operations
    /// </summary>
    public interface ILabelRepository : IGenericRepository<Label>
    {
        Task<IEnumerable<Label>> GetUserLabelsAsync(int userId, CancellationToken cancellationToken = default);
        Task<Label?> GetUserLabelByNameAsync(int userId, string labelName, CancellationToken cancellationToken = default);
        Task<bool> LabelExistsForUserAsync(int userId, string labelName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Label>> GetLabelsForNoteAsync(int noteId, CancellationToken cancellationToken = default);
        Task AddLabelToNoteAsync(int noteId, int labelId, CancellationToken cancellationToken = default);
        Task RemoveLabelFromNoteAsync(int noteId, int labelId, CancellationToken cancellationToken = default);
        Task RemoveAllNoteLabelAssociationsAsync(int labelId, CancellationToken cancellationToken = default);
        Task<int> GetLabelUsageCountAsync(int labelId, CancellationToken cancellationToken = default);
    }
}
