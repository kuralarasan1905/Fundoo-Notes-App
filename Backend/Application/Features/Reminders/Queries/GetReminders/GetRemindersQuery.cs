using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Reminders.Queries.GetReminders
{
    public class GetRemindersQuery : IRequest<List<NoteReminderDto>>
    {
        public int UserId { get; set; }
    }
}
