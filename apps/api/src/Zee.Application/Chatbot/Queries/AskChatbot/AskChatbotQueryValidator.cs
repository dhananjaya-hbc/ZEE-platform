using FluentValidation;

namespace Zee.Application.Chatbot.Queries.AskChatbot;

/// <summary>Shape validation for <see cref="AskChatbotQuery"/>.</summary>
///
/// WARNING: this validator currently enforces NOTHING.
///
/// TODO: Implement the rules.
/// Acceptance criteria:
///   - Question: NotEmpty, MaximumLength(MaxQuestionLength).
///
/// The length cap matters more here than on other endpoints. In Phase 2 this text goes into
/// a model prompt, where input length maps directly to cost and latency, and an unbounded
/// field is also the front door for prompt-injection payloads. Capping it in Phase 1 means
/// the limit is already enforced and already respected by clients before any of that is
/// live - much easier than tightening a limit people already depend on.
public sealed class AskChatbotQueryValidator : AbstractValidator<AskChatbotQuery>
{
    /// <summary>Longest question the assistant will accept.</summary>
    public const int MaxQuestionLength = 1000;

    public AskChatbotQueryValidator()
    {
        // Rules go here. See the acceptance criteria above.
    }
}
