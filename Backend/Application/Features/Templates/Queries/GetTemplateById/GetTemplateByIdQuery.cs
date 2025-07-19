using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Templates.Queries.GetTemplateById
{
    public class GetTemplateByIdQuery : IRequest<NoteTemplateDto?>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
