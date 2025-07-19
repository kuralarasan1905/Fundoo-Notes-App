using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Attachments.Queries.GetAttachments
{
    public class GetAttachmentsQueryHandler : IRequestHandler<GetAttachmentsQuery, List<NoteAttachmentDto>>
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public GetAttachmentsQueryHandler(
            IAttachmentRepository attachmentRepository,
            INoteRepository noteRepository,
            IMapper mapper)
        {
            _attachmentRepository = attachmentRepository;
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<List<NoteAttachmentDto>> Handle(GetAttachmentsQuery request, CancellationToken cancellationToken)
        {
            // Verify user has access to the note
            var note = await _noteRepository.GetByIdAsync(request.NoteId);
            if (note == null || note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            var attachments = await _attachmentRepository.GetByNoteIdAsync(request.NoteId);
            return _mapper.Map<List<NoteAttachmentDto>>(attachments);
        }
    }
}
