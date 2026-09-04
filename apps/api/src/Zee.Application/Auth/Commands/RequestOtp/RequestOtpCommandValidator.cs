using FluentValidation;

namespace Zee.Application.Auth.Commands.RequestOtp;

/// <summary>
/// Shape validation for <see cref="RequestOtpCommand"/>, run automatically by
/// <c>ValidationBehaviour</c> before the handler.
/// </summary>
/// <remarks>
/// This checks that the input LOOKS like an email address - the authoritative check
/// (does it belong to an onboarded university's verified domain) happens in the handler,
/// because that requires a database lookup a validator has no business doing.
/// </remarks>
public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("An email address is required.")
            .EmailAddress()
            .WithMessage("That does not look like a valid email address.")
            .MaximumLength(254)
            .WithMessage("Email address is too long.");
    }
}
