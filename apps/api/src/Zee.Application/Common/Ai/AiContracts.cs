// The wire contract between the .NET API and /apps/ai-service.
//
// These records mirror the FastAPI response models exactly. They are the shared
// vocabulary of the two services, so changing one side without the other breaks the
// boundary — the Python schemas in apps/ai-service/app/schemas/ are the other half and
// must move together.
//
// In Phase 1 every one of these is populated with mock data. The shapes are nonetheless
// real: Phase 2 replaces the implementation behind them, not the contract, so the .NET
// side needs no changes when the AI logic arrives.

namespace Zee.Application.Common.Ai;

/// <summary>One document the chatbot drew on when answering.</summary>
/// <param name="Id">Stable identifier of the source material.</param>
/// <param name="Title">Human-readable label to show under the answer.</param>
/// <param name="Url">Deep link into ZEE or an external page, when one exists.</param>
/// <remarks>
/// Citations are part of the contract from day one rather than bolted on in Phase 2. A
/// campus assistant that answers "the deadline is Friday" with no way to check where that
/// came from is worse than useless when it is wrong, and retrofitting sources into a UI
/// built without them is far more work than leaving room now.
/// </remarks>
public sealed record ChatbotSource(string Id, string Title, string? Url);

/// <summary>An answer from the campus assistant.</summary>
/// <param name="Answer">Natural-language response to show the student.</param>
/// <param name="Sources">Supporting documents. May be empty, never null.</param>
public sealed record ChatbotAnswer(string Answer, IReadOnlyList<ChatbotSource> Sources)
{
    /// <summary>Returned when the AI service is unreachable, so the caller still has something to render.</summary>
    public static ChatbotAnswer Unavailable { get; } = new(
        "The campus assistant is unavailable right now. Please try again shortly.",
        []);
}

/// <summary>A suggested student connection.</summary>
/// <param name="UserId">The student being recommended.</param>
/// <param name="Score">Confidence in the match, 0.0 to 1.0.</param>
/// <param name="Reason">
/// Short human-readable justification, e.g. "Also taking CS3300 and interested in robotics".
/// </param>
/// <remarks>
/// <paramref name="Reason"/> is required rather than optional. An unexplained "you should
/// meet this person" is uncomfortable and unactionable; forcing the AI service to say why
/// keeps Phase 2 honest about whether its matches make sense.
/// </remarks>
public sealed record StudentMatch(Guid UserId, double Score, string Reason);

/// <summary>A post id with its relevance score, as returned by the Phase 2 ranked feed.</summary>
/// <param name="PostId">Which post.</param>
/// <param name="Score">Relevance for this reader, higher is more relevant.</param>
/// <remarks>
/// The AI service returns ids and scores only - never post bodies. It does not own post
/// content and must not become a second source of truth for it. The API hydrates these ids
/// from PostgreSQL via <c>IPostRepository.GetByIdsAsync</c>, which also means visibility
/// rules are applied by the service that actually knows them.
/// </remarks>
public sealed record RankedPost(Guid PostId, double Score);
