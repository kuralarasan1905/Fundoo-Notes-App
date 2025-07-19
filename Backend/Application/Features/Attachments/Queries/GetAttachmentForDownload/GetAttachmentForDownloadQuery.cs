using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Attachments.Queries.GetAttachmentForDownload
{
    public class GetAttachmentForDownloadQuery : IRequest<AttachmentDownloadDto?>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
