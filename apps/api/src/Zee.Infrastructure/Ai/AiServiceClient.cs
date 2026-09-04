using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zee.Application.Common.Ai;
using Zee.Application.Common.Interfaces;

namespace Zee.Infrastructure.Ai;

/// <summary>
/// HTTP implementation of <see cref="IAiServiceClient"/>, talking to the FastAPI service.
/// </summary>
/// <remarks>
/// Registered as a typed client, so the <see cref="HttpClient"/> arrives pre-configured with
/// the base address, the timeout and the <c>X-Internal-Key</c> header - see
/// <c>DependencyInjection.AddInfrastructure</c>. Individual methods never touch that header.
///
/// <para><b>The failure contract is the important part of this class.</b> Every method
/// returns a fallback rather than throwing, because nothing here is on the critical path of
/// Phase 1: the chatbot degrades to an "unavailable" message, recommendations and rankings
/// degrade to empty. A student must never see the platform break because an optional
/// enrichment service is down.</para>
///
/// <para>In Phase 1 the responses are mock data, but this entire path is real - HTTP,
/// auth header, timeout, JSON deserialisation. Phase 2 changes the Python side only.</para>
/// </remarks>
public sealed class AiServiceClient(HttpClient http, ILogger<AiServiceClient> logger)
    : IAiServiceClient
{
    /// <summary>
    /// The Python service uses snake_case (<c>user_id</c>, <c>post_id</c>); .NET uses
    /// PascalCase. This is where the two conventions are reconciled.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http = http;
    private readonly ILogger<AiServiceClient> _logger = logger;

    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - POST to "/api/chatbot/ask" with body { question, user_id }.
    ///   - Deserialise into ChatbotAnswer using JsonOptions.
    ///   - On HttpRequestException, TaskCanceledException (timeout), JsonException, or a
    ///     non-success status: log a WARNING and return ChatbotAnswer.Unavailable.
    ///     Do NOT let the exception escape - see the class remarks.
    ///   - Do not log the question body. It is student input and may be personal.
    public Task<ChatbotAnswer> AskAsync(
        string question,
        Guid userId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement.
    ///   - GET "/api/recommendations/{userId}".
    ///   - Return the matches, or an EMPTY LIST on any failure.
    public Task<IReadOnlyList<StudentMatch>> GetRecommendationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement.
    ///   - GET "/api/feed/{userId}".
    ///   - Return ranked post ids, or an EMPTY LIST on any failure. An empty list is the
    ///     signal the feed handler reads as "fall back to chronological", which is why it
    ///     must never be an exception instead.
    ///   - Unused in Phase 1; the Phase 1 feed handler does not call this.
    public Task<IReadOnlyList<RankedPost>> GetRankedFeedAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
