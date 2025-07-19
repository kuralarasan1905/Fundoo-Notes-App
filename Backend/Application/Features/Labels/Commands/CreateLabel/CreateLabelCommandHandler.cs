using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Labels.Commands.CreateLabel
{
    /// <summary>
    /// Handler for CreateLabelCommand
    /// </summary>
    public class CreateLabelCommandHandler : IRequestHandler<CreateLabelCommand, LabelDto>
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateLabelCommandHandler> _logger;

        public CreateLabelCommandHandler(
            ILabelRepository labelRepository,
            IMapper mapper,
            ILogger<CreateLabelCommandHandler> logger)
        {
            _labelRepository = labelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<LabelDto> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating label for user: {UserId}", request.UserId);

            // Check if label with same name already exists for this user
            var labelExists = await _labelRepository.LabelExistsForUserAsync(request.UserId, request.Name, cancellationToken);
            if (labelExists)
            {
                throw new InvalidOperationException($"Label with name '{request.Name}' already exists");
            }

            var label = new Label
            {
                Name = request.Name,
                Color = request.Color,
                UserId = request.UserId
            };

            await _labelRepository.AddAsync(label, cancellationToken);
            await _labelRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Label {LabelId} created successfully for user: {UserId}", label.Id, request.UserId);

            return _mapper.Map<LabelDto>(label);
        }
    }
}
