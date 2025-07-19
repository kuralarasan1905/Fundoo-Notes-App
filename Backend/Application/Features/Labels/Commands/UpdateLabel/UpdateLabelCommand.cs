using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Labels.Commands.UpdateLabel
{
    /// <summary>
    /// Command for updating an existing label
    /// </summary>
    public class UpdateLabelCommand : IRequest<LabelDto>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
