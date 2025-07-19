using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Attachments.Queries.GetAttachmentForDownload
{
    public class GetAttachmentForDownloadQueryHandler : IRequestHandler<GetAttachmentForDownloadQuery, AttachmentDownloadDto?>
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public GetAttachmentForDownloadQueryHandler(
            IAttachmentRepository attachmentRepository,
            INoteRepository noteRepository,
            IMapper mapper)
        {
            _attachmentRepository = attachmentRepository;
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<AttachmentDownloadDto?> Handle(GetAttachmentForDownloadQuery request, CancellationToken cancellationToken)
        {
            var attachment = await _attachmentRepository.GetByIdWithNoteAsync(request.Id);
            if (attachment == null || attachment.Note.UserId != request.UserId)
            {
                return null;
            }

            return new AttachmentDownloadDto
            {
                FileName = attachment.FileName,
                ContentType = attachment.FileType,
                FilePath = attachment.FileUrl
            };
        }
    }
}
