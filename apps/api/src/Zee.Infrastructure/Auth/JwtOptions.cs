using System.ComponentModel.DataAnnotations;

namespace Zee.Infrastructure.Auth;

/// <summary>
/// Configuration for issuing and signing student JWTs. Bound from the <c>Jwt</c>
/// configuration section.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Symmetric signing key (HMAC-SHA256). Must be at least 32 characters (256 bits) -
    /// anything shorter is within reach of brute force against the algorithm.
    /// </summary>
    [Required]
    [MinLength(32)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Issuer and audience, both set to this same value. ZEE issues and consumes its own
    /// tokens - there is no third party audience to distinguish.
    /// </summary>
    [Required]
    public string Issuer { get; set; } = "zee";

    /// <summary>How long an issued access token remains valid.</summary>
    [Range(1, 1440)]
    public int AccessTokenLifetimeMinutes { get; set; } = 60;
}
