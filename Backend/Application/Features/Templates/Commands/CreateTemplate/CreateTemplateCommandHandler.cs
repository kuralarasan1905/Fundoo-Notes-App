using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Domain.Entities;
using AutoMapper;

namespace fundoo_notes.Application.Features.Templates.Commands.CreateTemplate
{
    public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, NoteTemplateDto>
    {
        private readonly INoteTemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public CreateTemplateCommandHandler(
            INoteTemplateRepository templateRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<NoteTemplateDto> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = new NoteTemplate
            {
                Name = request.Name,
                Description = request.Description,
                Title = request.Title,
                Content = request.Content,
                Color = request.Color,
                Category = request.Category,
                IsPublic = request.IsPublic,
                UserId = request.UserId
            };

            await _templateRepository.AddAsync(template);
            await _templateRepository.SaveChangesAsync();

            return _mapper.Map<NoteTemplateDto>(template);
        }
    }
}
