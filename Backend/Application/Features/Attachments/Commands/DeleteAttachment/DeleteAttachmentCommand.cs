using MediatR;

namespace fundoo_notes.Application.Features.Attachments.Commands.DeleteAttachment
{
    public class DeleteAttachmentCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
