using MediatR;

namespace fundoo_notes.Application.Features.Templates.Commands.DeleteTemplate
{
    public class DeleteTemplateCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
