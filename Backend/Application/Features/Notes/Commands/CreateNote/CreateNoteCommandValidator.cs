using FluentValidation;

namespace fundoo_notes.Application.Features.Notes.Commands.CreateNote
{
    /// <summary>
    /// Validator for CreateNoteCommand
    /// </summary>
    public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
    {
        public CreateNoteCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .Length(1, 200).WithMessage("Title must be between 1 and 200 characters");

            RuleFor(x => x.Content)
                .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters");

            RuleFor(x => x.Color)
                .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color code")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.ReminderDateTime)
                .GreaterThan(DateTime.UtcNow).WithMessage("Reminder date must be in the future")
                .When(x => x.ReminderDateTime.HasValue);

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID is required");
        }
    }
}
