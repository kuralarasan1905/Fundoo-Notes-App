using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Templates.Commands.CreateNoteFromTemplate
{
    public class CreateNoteFromTemplateCommand : IRequest<NoteDto>
    {
        public int TemplateId { get; set; }
        public int UserId { get; set; }
    }
}
