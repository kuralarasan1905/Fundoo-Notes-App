using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Users.Commands.LoginUser
{
    /// <summary>
    /// Command for user login
    /// </summary>
    public class LoginUserCommand : IRequest<AuthResponseDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
