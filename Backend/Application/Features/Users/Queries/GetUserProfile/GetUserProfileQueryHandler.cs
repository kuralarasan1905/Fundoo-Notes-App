using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Users.Queries.GetUserProfile
{
    /// <summary>
    /// Handler for GetUserProfileQuery
    /// </summary>
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            IUserRepository userRepository,
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            IMapper mapper,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _userRepository = userRepository;
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting profile for user: {UserId}", request.UserId);

            // Get user by ID
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Profile request failed: User not found: {UserId}", request.UserId);
                throw new ArgumentException("User not found");
            }

            // Get additional profile data
            var totalNotes = await _noteRepository.GetUserNotesCountAsync(request.UserId, cancellationToken);
            var totalLabels = (await _labelRepository.GetUserLabelsAsync(request.UserId, cancellationToken)).Count();

            // Create profile DTO
            var profileDto = _mapper.Map<UserProfileDto>(user);
            profileDto.TotalNotes = totalNotes;
            profileDto.TotalLabels = totalLabels;

            _logger.LogInformation("Profile retrieved successfully for user: {UserId}", request.UserId);

            return profileDto;
        }
    }
}
