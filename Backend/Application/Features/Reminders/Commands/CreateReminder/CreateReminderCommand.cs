using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Reminders.Commands.CreateReminder
{
    public class CreateReminderCommand : IRequest<NoteReminderDto>
    {
        public int NoteId { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public string ReminderType { get; set; } = "Once";
        public int UserId { get; set; }
    }
}
