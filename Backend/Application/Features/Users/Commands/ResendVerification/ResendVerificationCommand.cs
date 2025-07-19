using MediatR;

namespace fundoo_notes.Application.Features.Users.Commands.ResendVerification
{
    /// <summary>
    /// Command for resending email verification
    /// </summary>
    public class ResendVerificationCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
    }
}
