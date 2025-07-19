using MediatR;

namespace fundoo_notes.Application.Features.Users.Commands.VerifyEmail
{
    /// <summary>
    /// Command for verifying email address
    /// </summary>
    public class VerifyEmailCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
