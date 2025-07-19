using MediatR;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Attachments.Commands.DeleteAttachment
{
    public class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand, bool>
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly INoteRepository _noteRepository;

        public DeleteAttachmentCommandHandler(
            IAttachmentRepository attachmentRepository,
            INoteRepository noteRepository)
        {
            _attachmentRepository = attachmentRepository;
            _noteRepository = noteRepository;
        }

        public async Task<bool> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(request.Id);
            if (attachment == null)
            {
                return false;
            }

            // Verify user owns the note that contains this attachment
            var note = await _noteRepository.GetByIdAsync(attachment.NoteId);
            if (note == null || note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            // Delete physical file
            if (File.Exists(attachment.FileUrl))
            {
                File.Delete(attachment.FileUrl);
            }

            // Delete from database
            await _attachmentRepository.DeleteAsync(attachment.Id);
            await _attachmentRepository.SaveChangesAsync();

            return true;
        }
    }
}
