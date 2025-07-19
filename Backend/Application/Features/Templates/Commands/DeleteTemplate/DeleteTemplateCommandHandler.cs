using MediatR;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Templates.Commands.DeleteTemplate
{
    public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, bool>
    {
        private readonly INoteTemplateRepository _templateRepository;

        public DeleteTemplateCommandHandler(INoteTemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.Id);
            if (template == null || template.UserId != request.UserId)
            {
                return false;
            }

            await _templateRepository.DeleteAsync(template.Id);
            await _templateRepository.SaveChangesAsync();

            return true;
        }
    }
}
