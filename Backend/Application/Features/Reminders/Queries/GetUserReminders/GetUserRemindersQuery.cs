using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Reminders.Queries.GetUserReminders
{
    public class GetUserRemindersQuery : IRequest<List<NoteReminderDto>>
    {
        public int UserId { get; set; }
        public bool IncludeCompleted { get; set; } = false;
    }
}
