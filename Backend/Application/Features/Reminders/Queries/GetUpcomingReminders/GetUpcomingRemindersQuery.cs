using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Reminders.Queries.GetUpcomingReminders
{
    public class GetUpcomingRemindersQuery : IRequest<List<NoteReminderDto>>
    {
        public int UserId { get; set; }
        public DateTime? BeforeDateTime { get; set; }
    }
}
