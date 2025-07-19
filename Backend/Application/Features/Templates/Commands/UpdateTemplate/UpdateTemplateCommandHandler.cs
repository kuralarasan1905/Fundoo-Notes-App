using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Templates.Commands.UpdateTemplate
{
    public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, NoteTemplateDto>
    {
        private readonly INoteTemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public UpdateTemplateCommandHandler(
            INoteTemplateRepository templateRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<NoteTemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.Id);
            if (template == null || template.UserId != request.UserId)
            {
                throw new ArgumentException("Template not found or access denied");
            }

            template.Name = request.Name;
            template.Description = request.Description;
            template.Title = request.Title;
            template.Content = request.Content;
            template.Color = request.Color;
            template.Category = request.Category;
            template.IsPublic = request.IsPublic;
            template.UpdatedAt = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template);
            await _templateRepository.SaveChangesAsync();

            return _mapper.Map<NoteTemplateDto>(template);
        }
    }
}
