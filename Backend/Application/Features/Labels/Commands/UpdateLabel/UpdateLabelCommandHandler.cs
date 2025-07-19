using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Application.Features.Labels.Commands.UpdateLabel;

namespace fundoo_notes.Application.Features.Labels.Commands.UpdateLabel
{
    /// <summary>
    /// Handler for UpdateLabelCommand
    /// </summary>
    public class UpdateLabelCommandHandler : IRequestHandler<UpdateLabelCommand, LabelDto>
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateLabelCommandHandler> _logger;

        public UpdateLabelCommandHandler(
            ILabelRepository labelRepository,
            IMapper mapper,
            ILogger<UpdateLabelCommandHandler> logger)
        {
            _labelRepository = labelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<LabelDto> Handle(UpdateLabelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating label {LabelId} for user: {UserId}", request.Id, request.UserId);

            // Get the existing label
            var label = await _labelRepository.GetByIdAsync(request.Id, cancellationToken);
            if (label == null)
            {
                throw new KeyNotFoundException($"Label with ID {request.Id} not found");
            }

            // Check if user owns the label
            if (label.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this label");
            }

            // Check if another label with the same name already exists for this user (excluding current label)
            var labelExists = await _labelRepository.LabelExistsForUserAsync(request.UserId, request.Name, cancellationToken);
            if (labelExists && !label.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Label with name '{request.Name}' already exists");
            }

            // Update label properties
            label.Name = request.Name;
            label.Color = request.Color;

            await _labelRepository.UpdateAsync(label, cancellationToken);
            await _labelRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Label {LabelId} updated successfully for user: {UserId}", request.Id, request.UserId);

            return _mapper.Map<LabelDto>(label);
        }
    }
}
