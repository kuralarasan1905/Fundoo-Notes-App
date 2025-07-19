using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Users.Commands.RegisterUser
{
    /// <summary>
    /// Command for user registration
    /// </summary>
    public class RegisterUserCommand : IRequest<AuthResponseDto>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
