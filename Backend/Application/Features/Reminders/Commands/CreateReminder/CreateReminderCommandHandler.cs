using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Domain.Entities;
using AutoMapper;

namespace fundoo_notes.Application.Features.Reminders.Commands.CreateReminder
{
    public class CreateReminderCommandHandler : IRequestHandler<CreateReminderCommand, NoteReminderDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public CreateReminderCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<NoteReminderDto> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
        {
            // Verify note exists and belongs to user
            var note = await _noteRepository.GetByIdAsync(request.NoteId);
            if (note == null || note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Note not found or access denied");
            }

            // Update note's reminder
            note.ReminderDateTime = request.ReminderDateTime;
            await _noteRepository.UpdateAsync(note);
            await _noteRepository.SaveChangesAsync();

            // Return a basic reminder DTO
            return new NoteReminderDto
            {
                Id = note.Id,
                NoteId = note.Id,
                ReminderDateTime = request.ReminderDateTime,
                ReminderType = request.ReminderType,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                // Add note details for frontend display
                Title = note.Title,
                Content = note.Content,
                Color = note.Color
            };
        }
    }
}
