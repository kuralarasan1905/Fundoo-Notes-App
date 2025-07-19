using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Templates.Queries.GetPublicTemplates
{
    public class GetPublicTemplatesQueryHandler : IRequestHandler<GetPublicTemplatesQuery, List<NoteTemplateDto>>
    {
        private readonly INoteTemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public GetPublicTemplatesQueryHandler(
            INoteTemplateRepository templateRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<List<NoteTemplateDto>> Handle(GetPublicTemplatesQuery request, CancellationToken cancellationToken)
        {
            var templates = await _templateRepository.GetPublicTemplatesAsync();
            return _mapper.Map<List<NoteTemplateDto>>(templates);
        }
    }
}
