using FluentValidation;

namespace fundoo_notes.Application.Features.Users.Commands.RegisterUser
{
    /// <summary>
    /// Validator for RegisterUserCommand
    /// </summary>
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 100).WithMessage("First name must be between 2 and 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 100).WithMessage("Last name must be between 2 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Password confirmation is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}
