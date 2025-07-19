using MediatR;

namespace fundoo_notes.Application.Features.Users.Commands.DeactivateAccount
{
    /// <summary>
    /// Command for deactivating user account
    /// </summary>
    public class DeactivateAccountCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
