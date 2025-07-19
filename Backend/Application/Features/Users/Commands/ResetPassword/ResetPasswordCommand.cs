using MediatR;

namespace fundoo_notes.Application.Features.Users.Commands.ResetPassword
{
    /// <summary>
    /// Command for resetting password with token
    /// </summary>
    public class ResetPasswordCommand : IRequest<bool>
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
