using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Reminders.Commands.UpdateReminder
{
    public class UpdateReminderCommand : IRequest<NoteReminderDto>
    {
        public int Id { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public string ReminderType { get; set; } = "Once";
        public int UserId { get; set; }
    }
}
