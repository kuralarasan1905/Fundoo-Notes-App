using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Templates.Commands.CreateTemplate
{
    public class CreateTemplateCommand : IRequest<NoteTemplateDto>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
        public int UserId { get; set; }
    }
}
