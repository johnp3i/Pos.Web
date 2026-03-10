using FluentValidation;
using Pos.Web.Shared.DTOs.Authentication;

namespace Pos.Web.API.Validators;

/// <summary>
/// Validator for RefreshTokenRequestDto to ensure refresh token is provided.
/// </summary>
public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MinimumLength(20).WithMessage("Invalid refresh token format")
            .MaximumLength(500).WithMessage("Invalid refresh token format");
    }
}
