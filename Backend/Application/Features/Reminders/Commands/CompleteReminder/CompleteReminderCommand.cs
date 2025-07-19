using MediatR;

namespace fundoo_notes.Application.Features.Reminders.Commands.CompleteReminder
{
    public class CompleteReminderCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
