using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Labels.Commands.CreateLabel
{
    /// <summary>
    /// Command for creating a new label
    /// </summary>
    public class CreateLabelCommand : IRequest<LabelDto>
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
