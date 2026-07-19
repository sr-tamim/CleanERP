using FluentValidation;

namespace CleanERP.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Validator for login command
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty()
            .WithMessage("Username or email is required")
            .MaximumLength(255)
            .WithMessage("Username or email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(1)
            .WithMessage("Password is required");
    }
}
