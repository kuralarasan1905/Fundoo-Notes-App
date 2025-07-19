using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Users.Queries.GetUserProfile
{
    /// <summary>
    /// Query for getting user profile
    /// </summary>
    public class GetUserProfileQuery : IRequest<UserProfileDto>
    {
        public int UserId { get; set; }
    }
}
