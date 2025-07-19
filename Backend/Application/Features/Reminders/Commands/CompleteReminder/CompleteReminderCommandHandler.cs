using MediatR;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Reminders.Commands.CompleteReminder
{
    public class CompleteReminderCommandHandler : IRequestHandler<CompleteReminderCommand, bool>
    {
        private readonly INoteRepository _noteRepository;

        public CompleteReminderCommandHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<bool> Handle(CompleteReminderCommand request, CancellationToken cancellationToken)
        {
            // Verify note exists and belongs to user
            var note = await _noteRepository.GetByIdAsync(request.Id);
            if (note == null || note.UserId != request.UserId)
            {
                return false;
            }

            // Mark reminder as completed by removing it
            note.ReminderDateTime = null;
            await _noteRepository.UpdateAsync(note);
            await _noteRepository.SaveChangesAsync();

            return true;
        }
    }
}
