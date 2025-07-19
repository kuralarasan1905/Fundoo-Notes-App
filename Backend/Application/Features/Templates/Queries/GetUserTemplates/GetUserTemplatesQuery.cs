using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Templates.Queries.GetUserTemplates
{
    public class GetUserTemplatesQuery : IRequest<List<NoteTemplateDto>>
    {
        public int UserId { get; set; }
        public string? Category { get; set; }
    }
}
