using MediatR;

namespace fundoo_notes.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Command for changing user password
    /// </summary>
    public class ChangePasswordCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
