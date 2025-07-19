using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Queries.GetPublicTemplateQuery
{
    public class GetPublicTemplateQuery : IRequest<List<NoteTemplateDto>>
    {
    }
}
