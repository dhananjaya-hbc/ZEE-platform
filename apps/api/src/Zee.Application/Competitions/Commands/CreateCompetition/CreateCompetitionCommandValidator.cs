using FluentValidation;

namespace Zee.Application.Competitions.Commands.CreateCompetition;

/// <summary>Shape validation for <see cref="CreateCompetitionCommand"/>.</summary>
///
/// WARNING: this validator currently enforces NOTHING.
///
/// TODO: Implement the rules.
/// Acceptance criteria:
///   - Title: NotEmpty, MaximumLength(200).
///   - Description: NotEmpty, MaximumLength(10000).
///   - Category: IsInEnum.
///   - EndDate: GreaterThan(StartDate).
///   - StartDate: not in the past, with a small tolerance for clock skew.
public sealed class CreateCompetitionCommandValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionCommandValidator()
    {
        // Rules go here. See the acceptance criteria above.
    }
}
