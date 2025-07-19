using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Domain.Entities;
using AutoMapper;

namespace fundoo_notes.Application.Features.Templates.Commands.CreateNoteFromTemplate
{
    public class CreateNoteFromTemplateCommandHandler : IRequestHandler<CreateNoteFromTemplateCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly INoteTemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public CreateNoteFromTemplateCommandHandler(
            INoteRepository noteRepository,
            INoteTemplateRepository templateRepository,
            IMapper mapper)
        {
            _noteRepository = noteRepository;
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<NoteDto> Handle(CreateNoteFromTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId);
            if (template == null)
            {
                throw new ArgumentException("Template not found");
            }

            var note = new Note
            {
                Title = template.Title,
                Content = template.Content,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsArchived = false,
                IsTrashed = false,
                IsPinned = false,
                Color = template.Color
            };

            await _noteRepository.AddAsync(note);
            await _noteRepository.SaveChangesAsync();

            return _mapper.Map<NoteDto>(note);
        }
    }
}
