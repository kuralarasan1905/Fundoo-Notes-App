using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Collaborators.Queries.GetCollaborators
{
    public class GetCollaboratorsQueryHandler : IRequestHandler<GetCollaboratorsQuery, List<CollaboratorDto>>
    {
        private readonly ICollaboratorRepository _collaboratorRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public GetCollaboratorsQueryHandler(
            ICollaboratorRepository collaboratorRepository,
            INoteRepository noteRepository,
            IMapper mapper)
        {
            _collaboratorRepository = collaboratorRepository;
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<List<CollaboratorDto>> Handle(GetCollaboratorsQuery request, CancellationToken cancellationToken)
        {
            // Verify user has access to the note
            var note = await _noteRepository.GetByIdAsync(request.NoteId);
            if (note == null || (note.UserId != request.UserId && !await _collaboratorRepository.IsCollaboratorAsync(request.NoteId, request.UserId)))
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            var collaborators = await _collaboratorRepository.GetByNoteIdAsync(request.NoteId);
            return _mapper.Map<List<CollaboratorDto>>(collaborators);
        }
    }
}
