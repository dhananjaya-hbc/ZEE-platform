using System.Security.Claims;
using Zee.Application.Common.Interfaces;

namespace Zee.Api.Services;

/// <summary>
/// Reads the authenticated student's identity from the current request's JWT claims.
/// </summary>
/// <remarks>
/// Lives in the Api layer because it is the only layer that knows what an HTTP request is.
/// Handlers depend on <see cref="ICurrentUser"/> and can be tested with a stub.
///
/// <para>Registered as scoped: it reads per-request state, and a singleton would capture
/// whichever request happened to be in flight first.</para>
/// </remarks>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor = accessor;

    public Guid? UserId => ParseGuidClaim(ClaimTypes.NameIdentifier);

    public Guid? UniversityId => ParseGuidClaim(ICurrentUser.UniversityIdClaimType);


    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Reads a claim and parses it as a GUID, returning null rather than throwing.
    /// </summary>
    /// <remarks>
    /// A malformed claim means an unusable identity, which callers already handle by
    /// treating null as "not authenticated" and failing closed. Throwing here would turn a
    /// bad token into a 500 instead of the 403 it should be.
    /// </remarks>
    private Guid? ParseGuidClaim(string claimType)
    {
        var value = _accessor.HttpContext?.User.FindFirstValue(claimType);

        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
