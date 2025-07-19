using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Domain.Entities;
using AutoMapper;

namespace fundoo_notes.Application.Features.Collaborators.Commands.AddCollaborator
{
    public class AddCollaboratorCommandHandler : IRequestHandler<AddCollaboratorCommand, CollaboratorDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ICollaboratorRepository _collaboratorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AddCollaboratorCommandHandler(
            INoteRepository noteRepository,
            ICollaboratorRepository collaboratorRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _noteRepository = noteRepository;
            _collaboratorRepository = collaboratorRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CollaboratorDto> Handle(AddCollaboratorCommand request, CancellationToken cancellationToken)
        {
            // Verify note exists and belongs to user
            var note = await _noteRepository.GetByIdAsync(request.NoteId);
            if (note == null || note.UserId != request.RequesterId)
            {
                throw new UnauthorizedAccessException("Note not found or access denied");
            }

            // Find user by email
            var collaboratorUser = await _userRepository.GetByEmailAsync(request.UserEmail);
            if (collaboratorUser == null)
            {
                throw new ArgumentException("User with this email does not exist");
            }

            // Check if already a collaborator
            var existingCollaborator = await _collaboratorRepository.GetByNoteAndUserAsync(request.NoteId, collaboratorUser.Id);
            if (existingCollaborator != null)
            {
                throw new InvalidOperationException("User is already a collaborator on this note");
            }

            // Create collaborator
            var collaborator = new Collaborator
            {
                NoteId = request.NoteId,
                UserId = collaboratorUser.Id,
                Permission = request.Permission
            };

            await _collaboratorRepository.AddAsync(collaborator);
            await _collaboratorRepository.SaveChangesAsync();

            var collaboratorDto = _mapper.Map<CollaboratorDto>(collaborator);
            collaboratorDto.UserEmail = collaboratorUser.Email;
            collaboratorDto.UserName = $"{collaboratorUser.FirstName} {collaboratorUser.LastName}";

            return collaboratorDto;
        }
    }
}
