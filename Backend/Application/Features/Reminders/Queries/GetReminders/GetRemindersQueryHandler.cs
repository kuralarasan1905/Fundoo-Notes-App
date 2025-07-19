using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Reminders.Queries.GetReminders
{
    public class GetRemindersQueryHandler : IRequestHandler<GetRemindersQuery, List<NoteReminderDto>>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public GetRemindersQueryHandler(
            INoteRepository noteRepository,
            IMapper mapper)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<List<NoteReminderDto>> Handle(GetRemindersQuery request, CancellationToken cancellationToken)
        {
            var notesWithReminders = await _noteRepository.GetNotesWithRemindersAsync();
            var userNotes = notesWithReminders.Where(n => n.UserId == request.UserId);

            var reminders = userNotes.Select(note => new NoteReminderDto
            {
                Id = note.Id,
                NoteId = note.Id,
                ReminderDateTime = note.ReminderDateTime ?? DateTime.MinValue,
                ReminderType = "Once",
                IsCompleted = false,
                CreatedAt = note.CreatedAt,
                // Add note details for frontend display
                Title = note.Title,
                Content = note.Content,
                Color = note.Color
            }).ToList();

            return reminders;
        }
    }
}
