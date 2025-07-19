using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.CreateNote
{
    public class CreateNoteCommand : IRequest<NoteDto>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Color { get; set; }
        public DateTime? ReminderDateTime { get; set; }
        public List<int> LabelIds { get; set; } = new List<int>();
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
