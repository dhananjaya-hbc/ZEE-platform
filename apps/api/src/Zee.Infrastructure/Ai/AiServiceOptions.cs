using System.ComponentModel.DataAnnotations;

namespace Zee.Infrastructure.Ai;

/// <summary>
/// Configuration for reaching <c>/apps/ai-service</c>. Bound from the <c>AiService</c>
/// configuration section.
/// </summary>
/// <remarks>
/// In Docker Compose these come from environment variables using the double-underscore
/// convention .NET uses for nested keys: <c>AiService__BaseUrl</c>,
/// <c>AiService__InternalKey</c>.
/// </remarks>
public sealed class AiServiceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "AiService";

    /// <summary>
    /// Base address of the AI service, e.g. <c>http://ai-service:8000</c> inside Compose.
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Shared secret sent as the <c>X-Internal-Key</c> header on every request.
    /// </summary>
    /// <remarks>
    /// This is the only thing standing between the AI service and anyone who can route to
    /// its port, so it must match <c>INTERNAL_API_KEY</c> on the Python side exactly. It is
    /// a service-to-service credential and must never be sent to a browser - all AI traffic
    /// is proxied through this API precisely so the key stays server-side.
    /// </remarks>
    [Required]
    [MinLength(16)]
    public string InternalKey { get; set; } = string.Empty;

    /// <summary>
    /// Per-request timeout in seconds.
    /// </summary>
    /// <remarks>
    /// Short on purpose. Every AI call is optional enrichment with a fallback, so waiting
    /// 30 seconds to learn the service is unwell is strictly worse than giving up in 5 and
    /// serving the chronological feed. Phase 2 may need a longer timeout on the chatbot
    /// specifically, which is a good reason to make this per-endpoint later.
    /// </remarks>
    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 5;
}
