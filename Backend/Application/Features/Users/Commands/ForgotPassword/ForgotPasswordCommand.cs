using MediatR;

namespace fundoo_notes.Application.Features.Users.Commands.ForgotPassword
{
    /// <summary>
    /// Command for requesting password reset
    /// </summary>
    public class ForgotPasswordCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
    }
}
