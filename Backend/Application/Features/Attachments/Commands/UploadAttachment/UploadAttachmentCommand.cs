using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Attachments.Commands.UploadAttachment
{
    public class UploadAttachmentCommand : IRequest<NoteAttachmentDto>
    {
        public int NoteId { get; set; }
        public IFormFile File { get; set; } = null!;
        public int UserId { get; set; }
    }
}
