using FluentValidation;
using Pos.Web.Shared.DTOs.Authentication;

namespace Pos.Web.API.Validators;

/// <summary>
/// Validator for ChangePasswordRequestDto to ensure password complexity requirements.
/// </summary>
public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestDtoValidator()
    {
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
    }

    private bool HaveAtLeastFourUniqueCharacters(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return password.Distinct().Count() >= 4;
    }
}
