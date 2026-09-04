namespace Zee.Application.Common.Interfaces;

/// <summary>
/// Who is making the current request.
/// </summary>
/// <remarks>
/// Implemented in the Api layer over <c>IHttpContextAccessor</c> and the JWT claims.
/// Handlers depend on this rather than on <c>HttpContext</c>, so a use case can be tested by
/// handing it a stub instead of standing up a request pipeline.
///
/// <para>Deliberately not a parameter on every command. If commands carried their own
/// <c>AuthorId</c>, a controller that forgot to overwrite it from the token would happily
/// let a caller post as someone else - an authorisation bug that looks like ordinary
/// plumbing. Taking identity from the authenticated principal instead means the client
/// simply cannot express "post as another user".</para>
/// </remarks>
public interface ICurrentUser
{
    /// <summary>The authenticated student's id, or null when the request is anonymous.</summary>
    Guid? UserId { get; }

    /// <summary>The authenticated student's campus, or null when the request is anonymous.</summary>
    Guid? UniversityId { get; }

    /// <summary>True when the request carries a valid token.</summary>
    bool IsAuthenticated { get; }
}
