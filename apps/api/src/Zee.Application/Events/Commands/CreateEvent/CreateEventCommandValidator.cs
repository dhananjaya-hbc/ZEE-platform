using FluentValidation;

namespace Zee.Application.Events.Commands.CreateEvent;

/// <summary>Shape validation for <see cref="CreateEventCommand"/>.</summary>
///
/// WARNING: this validator currently enforces NOTHING.
///
/// TODO: Implement the rules.
/// Acceptance criteria:
///   - Title: NotEmpty, MaximumLength(200).
///   - Location: NotEmpty, MaximumLength(300).
///   - Description: MaximumLength(5000) when present.
///   - EndTime: GreaterThan(StartTime).
///   - StartTime: must not be in the past. Allow a small tolerance (a few minutes) for
///     clock skew rather than comparing strictly against UtcNow.
///   - Consider capping how far ahead an event may be scheduled (say 2 years) - it catches
///     year typos like 2206 before they pin an event to the top of a sorted list forever.
public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        // Rules go here. See the acceptance criteria above.
    }
}
