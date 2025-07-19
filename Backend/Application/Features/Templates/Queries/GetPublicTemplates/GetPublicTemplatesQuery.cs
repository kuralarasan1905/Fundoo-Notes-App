using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Templates.Queries.GetPublicTemplates
{
    public class GetPublicTemplatesQuery : IRequest<List<NoteTemplateDto>>
    {
        public string? Category { get; set; }
    }
}
