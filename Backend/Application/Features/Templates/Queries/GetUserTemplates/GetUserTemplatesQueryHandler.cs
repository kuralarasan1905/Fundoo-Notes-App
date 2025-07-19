using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Templates.Queries.GetUserTemplates
{
    public class GetUserTemplatesQueryHandler : IRequestHandler<GetUserTemplatesQuery, List<NoteTemplateDto>>
    {
        private readonly INoteTemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public GetUserTemplatesQueryHandler(
            INoteTemplateRepository templateRepository,
            IMapper mapper)
        {
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<List<NoteTemplateDto>> Handle(GetUserTemplatesQuery request, CancellationToken cancellationToken)
        {
            var templates = await _templateRepository.GetByUserIdAsync(request.UserId);
            return _mapper.Map<List<NoteTemplateDto>>(templates);
        }
    }
}
