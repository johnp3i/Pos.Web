using FluentValidation;
using Pos.Web.Shared.DTOs.Authentication;

namespace Pos.Web.API.Validators;

/// <summary>
/// Validator for FirstLoginPasswordChangeRequestDto to ensure password complexity requirements.
/// </summary>
public class FirstLoginPasswordChangeRequestDtoValidator : AbstractValidator<FirstLoginPasswordChangeRequestDto>
{
    public FirstLoginPasswordChangeRequestDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, underscores, dots, and hyphens");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character")
            .Must(HaveAtLeastFourUniqueCharacters).WithMessage("Password must contain at least 4 unique characters");

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");

        RuleFor(x => x.DeviceType)
            .MaximumLength(50).WithMessage("Device type must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.DeviceType));
    }

    private bool HaveAtLeastFourUniqueCharacters(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return password.Distinct().Count() >= 4;
    }
}
