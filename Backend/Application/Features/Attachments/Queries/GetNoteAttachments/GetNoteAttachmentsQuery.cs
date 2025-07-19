using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Attachments.Queries.GetNoteAttachments
{
    public class GetNoteAttachmentsQuery : IRequest<List<NoteAttachmentDto>>
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
    }
}
