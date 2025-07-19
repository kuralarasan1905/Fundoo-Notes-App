using MediatR;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Reminders.Commands.DeleteReminder
{
    public class DeleteReminderCommandHandler : IRequestHandler<DeleteReminderCommand, bool>
    {
        private readonly INoteRepository _noteRepository;

        public DeleteReminderCommandHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<bool> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
        {
            // Verify note exists and belongs to user
            var note = await _noteRepository.GetByIdAsync(request.Id);
            if (note == null || note.UserId != request.UserId)
            {
                return false;
            }

            // Remove reminder from note
            note.ReminderDateTime = null;
            await _noteRepository.UpdateAsync(note);
            await _noteRepository.SaveChangesAsync();

            return true;
        }
    }
}
