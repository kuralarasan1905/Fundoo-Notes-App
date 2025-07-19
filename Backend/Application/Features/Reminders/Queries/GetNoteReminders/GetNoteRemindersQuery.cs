using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Reminders.Queries.GetNoteReminders
{
    public class GetNoteRemindersQuery : IRequest<List<NoteReminderDto>>
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
    }
}
