using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Labels.Queries.GetUserLabels
{
    /// <summary>
    /// Query for getting user's labels
    /// </summary>
    public class GetUserLabelsQuery : IRequest<List<LabelDto>>
    {
        public int UserId { get; set; }
    }
}
