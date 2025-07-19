using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Templates.Queries.GetTemplateById
{
    public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, NoteTemplateDto?>
    {
        private readonly INoteTemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public GetTemplateByIdQueryHandler(
            INoteTemplateRepository templateRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<NoteTemplateDto?> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.Id);
            if (template == null || (template.UserId != request.UserId && !template.IsPublic))
            {
                return null;
            }

            return _mapper.Map<NoteTemplateDto>(template);
        }
    }
}
