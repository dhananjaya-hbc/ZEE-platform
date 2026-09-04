using Zee.Application.Common.Ai;

namespace Zee.Application.Common.Interfaces;

/// <summary>
/// The Application layer's view of <c>/apps/ai-service</c>.
/// </summary>
/// <remarks>
/// Declared here, implemented in Infrastructure. Handlers depend on this interface and know
/// nothing about HTTP, the internal API key, retries or JSON - which is what lets them be
/// unit tested with a substitute and what lets the transport change without touching a use
/// case.
///
/// <para><b>Every method on this interface is allowed to fail, and callers must cope.</b>
/// The AI service is a separate process that can be down, slow, or mid-deploy. Nothing here
/// is on the critical path of Phase 1: the feed falls back to chronological, the chatbot
/// returns <see cref="ChatbotAnswer.Unavailable"/>, recommendations come back empty. A
/// student should never see the platform break because an optional enrichment service is
/// unavailable, so the implementation returns fallbacks rather than throwing.</para>
///
/// <para>In Phase 1 the service behind this returns mock data. Consumers cannot tell the
/// difference, and that is the point - the seam is real even while the logic is not.</para>
/// </remarks>
public interface IAiServiceClient
{
    /// <summary>
    /// Asks the campus assistant a question.
    /// </summary>
    /// <returns>
    /// An answer with its sources, or <see cref="ChatbotAnswer.Unavailable"/> if the
    /// service could not be reached.
    /// </returns>
    Task<ChatbotAnswer> AskAsync(
        string question,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests suggested student connections.
    /// </summary>
    /// <returns>Matches in descending score order, or an empty list on failure.</returns>
    Task<IReadOnlyList<StudentMatch>> GetRecommendationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a ranked ordering of feed posts for a student.
    /// </summary>
    /// <returns>
    /// Post ids with scores in rank order, or an empty list on failure - which the feed
    /// handler treats as "fall back to chronological".
    /// </returns>
    /// <remarks>
    /// Unused by the Phase 1 feed handler, which orders by <c>CreatedAt</c> unconditionally.
    /// It exists so the ranked path can be switched on in Phase 2 without a new interface.
    /// </remarks>
    Task<IReadOnlyList<RankedPost>> GetRankedFeedAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
