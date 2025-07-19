using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Templates.Queries.GetTemplates
{
    public class GetTemplatesQuery : IRequest<List<NoteTemplateDto>>
    {
        public int UserId { get; set; }
    }
}
