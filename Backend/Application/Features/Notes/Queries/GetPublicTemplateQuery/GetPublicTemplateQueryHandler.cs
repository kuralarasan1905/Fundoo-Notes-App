using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Notes.Queries.GetPublicTemplateQuery
{
    public class GetPublicTemplateQueryHandler : IRequestHandler<GetPublicTemplateQuery, List<NoteTemplateDto>>
    {
        private readonly INoteTemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public GetPublicTemplateQueryHandler(
            INoteTemplateRepository templateRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<List<NoteTemplateDto>> Handle(GetPublicTemplateQuery request, CancellationToken cancellationToken)
        {
            var publicTemplates = await _templateRepository.GetPublicTemplatesAsync();
            return _mapper.Map<List<NoteTemplateDto>>(publicTemplates);
        }
    }
}
