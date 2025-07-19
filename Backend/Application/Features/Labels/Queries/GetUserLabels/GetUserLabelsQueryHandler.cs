using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Labels.Queries.GetUserLabels
{
    /// <summary>
    /// Handler for GetUserLabelsQuery
    /// </summary>
    public class GetUserLabelsQueryHandler : IRequestHandler<GetUserLabelsQuery, List<LabelDto>>
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserLabelsQueryHandler> _logger;

        public GetUserLabelsQueryHandler(
            ILabelRepository labelRepository,
            IMapper mapper,
            ILogger<GetUserLabelsQueryHandler> logger)
        {
            _labelRepository = labelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<LabelDto>> Handle(GetUserLabelsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting labels for user: {UserId}", request.UserId);

            var labels = await _labelRepository.GetUserLabelsAsync(request.UserId, cancellationToken);

            _logger.LogInformation("Found {Count} labels for user: {UserId}", labels.Count(), request.UserId);

            return _mapper.Map<List<LabelDto>>(labels);
        }
    }
}
