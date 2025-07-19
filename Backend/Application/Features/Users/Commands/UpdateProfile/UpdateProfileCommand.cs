using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Users.Commands.UpdateProfile
{
    /// <summary>
    /// Command for updating user profile
    /// </summary>
    public class UpdateProfileCommand : IRequest<UserProfileDto>
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Bio { get; set; }
    }
}
