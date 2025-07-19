using MediatR;

namespace fundoo_notes.Application.Features.Reminders.Commands.DeleteReminder
{
    public class DeleteReminderCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
