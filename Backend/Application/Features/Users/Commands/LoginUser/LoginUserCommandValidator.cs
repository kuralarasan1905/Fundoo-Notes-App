using FluentValidation;

namespace fundoo_notes.Application.Features.Users.Commands.LoginUser
{
    /// <summary>
    /// Validator for LoginUserCommand
    /// </summary>
    public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
