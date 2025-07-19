using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Reminders.Queries.GetNoteReminders
{
    public class GetNoteRemindersQueryHandler : IRequestHandler<GetNoteRemindersQuery, List<NoteReminderDto>>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public GetNoteRemindersQueryHandler(
            INoteRepository noteRepository,
            IMapper mapper)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<List<NoteReminderDto>> Handle(GetNoteRemindersQuery request, CancellationToken cancellationToken)
        {
            // Verify user has access to the note
            var note = await _noteRepository.GetByIdAsync(request.NoteId);
            if (note == null || note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            // Return reminder for this specific note if it has one
            var reminders = new List<NoteReminderDto>();
            if (note.ReminderDateTime.HasValue)
            {
                reminders.Add(new NoteReminderDto
                {
                    Id = note.Id,
                    NoteId = note.Id,
                    ReminderDateTime = note.ReminderDateTime.Value,
                    ReminderType = "Once",
                    IsCompleted = false,
                    CreatedAt = note.CreatedAt,
                    // Add note details for frontend display
                    Title = note.Title,
                    Content = note.Content,
                    Color = note.Color
                });
            }

            return reminders;
        }
    }
}
