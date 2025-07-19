using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Application.Features.Labels.Commands.DeleteLabel;

namespace fundoo_notes.Application.Features.Labels.Commands.DeleteLabel
{
    /// <summary>
    /// Handler for DeleteLabelCommand
    /// </summary>
    public class DeleteLabelCommandHandler : IRequestHandler<DeleteLabelCommand, bool>
    {
        private readonly ILabelRepository _labelRepository;
        private readonly ILogger<DeleteLabelCommandHandler> _logger;

        public DeleteLabelCommandHandler(
            ILabelRepository labelRepository,
            ILogger<DeleteLabelCommandHandler> logger)
        {
            _labelRepository = labelRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting label {LabelId} for user: {UserId}", request.Id, request.UserId);

            // Get the existing label
            var label = await _labelRepository.GetByIdAsync(request.Id, cancellationToken);
            if (label == null)
            {
                _logger.LogWarning("Label {LabelId} not found for deletion", request.Id);
                throw new KeyNotFoundException($"Label with ID {request.Id} not found");
            }

            // Check if user owns the label
            if (label.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete label {LabelId} without permission", request.UserId, request.Id);
                throw new UnauthorizedAccessException("You don't have permission to delete this label");
            }

            // Remove all note-label associations first
            await _labelRepository.RemoveAllNoteLabelAssociationsAsync(request.Id, cancellationToken);

            // Delete the label
            await _labelRepository.DeleteAsync(label, cancellationToken);
            await _labelRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Label {LabelId} deleted successfully by user: {UserId}", request.Id, request.UserId);

            return true;
        }
    }
}
