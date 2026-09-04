namespace Zee.Application.Auth.Dtos;

/// <summary>What the API returns after a successful sign-in.</summary>
/// <param name="AccessToken">Signed JWT. The client sends this as a Bearer token on every subsequent request.</param>
public sealed record AuthResultDto(
    string AccessToken,
    Guid UserId,
    string Name,
    string Email,
    Guid UniversityId);
