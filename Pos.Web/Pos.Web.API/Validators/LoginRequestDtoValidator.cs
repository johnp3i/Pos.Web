using FluentValidation;
using Pos.Web.Shared.DTOs.Authentication;

namespace Pos.Web.API.Validators;

/// <summary>
/// Validator for LoginRequestDto to ensure username and password meet requirements.
/// </summary>
public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, underscores, dots, and hyphens");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.DeviceType)
            .MaximumLength(50).WithMessage("Device type must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.DeviceType));
    }
}
