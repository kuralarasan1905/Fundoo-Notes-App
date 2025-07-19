using MediatR;

namespace fundoo_notes.Application.Features.Labels.Commands.DeleteLabel
{
    /// <summary>
    /// Command for deleting a label
    /// </summary>
    public class DeleteLabelCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
