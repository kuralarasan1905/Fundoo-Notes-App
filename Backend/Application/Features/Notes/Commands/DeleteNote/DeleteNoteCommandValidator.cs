using FluentValidation;

namespace fundoo_notes.Application.Features.Notes.Commands.DeleteNote
{
    /// <summary>
    /// Validator for DeleteNoteCommand
    /// </summary>
    public class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
    {
        public DeleteNoteCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Note ID must be greater than 0");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID is required and must be greater than 0");
        }
    }
}
