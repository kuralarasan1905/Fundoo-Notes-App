using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Users.Commands.UpdateProfile
{
    /// <summary>
    /// Handler for UpdateProfileCommand
    /// </summary>
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProfileCommandHandler> _logger;

        public UpdateProfileCommandHandler(
            IUserRepository userRepository,
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            IMapper mapper,
            ILogger<UpdateProfileCommandHandler> logger)
        {
            _userRepository = userRepository;
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing profile update for user: {UserId}", request.UserId);

            // Get user by ID
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Profile update failed: User not found: {UserId}", request.UserId);
                throw new ArgumentException("User not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Profile update failed: User account is inactive: {UserId}", request.UserId);
                throw new ArgumentException("User account is inactive");
            }

            // Check if email is being changed and if new email already exists
            if (user.Email != request.Email)
            {
                var emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
                if (emailExists)
                {
                    _logger.LogWarning("Profile update failed: Email already exists: {Email}", request.Email);
                    throw new ArgumentException("Email address is already in use");
                }

                // If email is changed, mark as unverified
                user.IsEmailVerified = false;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
            }

            // Update user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.Bio = request.Bio;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated successfully for user: {UserId}", request.UserId);

            // Get additional profile data
            var totalNotes = await _noteRepository.GetUserNotesCountAsync(request.UserId, cancellationToken);
            var totalLabels = (await _labelRepository.GetUserLabelsAsync(request.UserId, cancellationToken)).Count();

            // Create profile DTO
            var profileDto = _mapper.Map<UserProfileDto>(user);
            profileDto.TotalNotes = totalNotes;
            profileDto.TotalLabels = totalLabels;

            return profileDto;
        }
    }
}
