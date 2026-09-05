using FluentValidation;

namespace Zee.Application.Auth.Commands.VerifyOtp;

/// <summary>Shape validation for <see cref="VerifyOtpCommand"/>.</summary>
public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("An email address is required.")
            .EmailAddress()
            .WithMessage("That does not look like a valid email address.");

        RuleFor(c => c.Code)
            .NotEmpty()
            .WithMessage("A code is required.")
            .Matches(@"^\d{6}$")
            .WithMessage("The code must be exactly 6 digits.");
    }
}
