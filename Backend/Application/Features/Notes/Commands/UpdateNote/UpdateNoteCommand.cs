using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.UpdateNote
{
    /// <summary>
    /// Command for updating an existing note
    /// </summary>
    public class UpdateNoteCommand : IRequest<NoteDto>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Color { get; set; }
        public DateTime? ReminderDateTime { get; set; }
        public List<int>? LabelIds { get; set; } = null;
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
