using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Domain.Entities;
using AutoMapper;

namespace fundoo_notes.Application.Features.Attachments.Commands.UploadAttachment
{
    public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, NoteAttachmentDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IMapper _mapper;

        public UploadAttachmentCommandHandler(
            INoteRepository noteRepository,
            IAttachmentRepository attachmentRepository,
            IMapper mapper)
        {
            _noteRepository = noteRepository;
            _attachmentRepository = attachmentRepository;
            _mapper = mapper;
        }

        public async Task<NoteAttachmentDto> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
        {
            // Verify note exists and belongs to user
            var note = await _noteRepository.GetByIdAsync(request.NoteId);
            if (note == null || note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Note not found or access denied");
            }

            // Save file to storage
            var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
            var filePath = Path.Combine("uploads", "attachments", fileName);
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            // Create attachment entity
            var attachment = new NoteAttachment
            {
                NoteId = request.NoteId,
                FileName = request.File.FileName,
                FileUrl = filePath,
                FileSize = request.File.Length,
                FileType = request.File.ContentType ?? "application/octet-stream"
            };

            await _attachmentRepository.AddAsync(attachment);
            await _attachmentRepository.SaveChangesAsync();

            return _mapper.Map<NoteAttachmentDto>(attachment);
        }
    }
}
