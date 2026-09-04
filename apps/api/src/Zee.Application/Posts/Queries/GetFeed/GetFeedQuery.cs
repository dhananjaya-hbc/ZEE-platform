using MediatR;
using Zee.Application.Common.Models;
using Zee.Application.Posts.Dtos;

namespace Zee.Application.Posts.Queries.GetFeed;

/// <summary>
/// Fetches one page of the authenticated student's feed.
/// </summary>
/// <param name="Cursor">
/// Opaque cursor from a previous page's <c>nextCursor</c>, or null to start at the newest post.
/// </param>
/// <param name="Limit">Maximum posts to return. Clamped by the validator.</param>
/// <remarks>
/// <b>Phase 1 returns posts in reverse-chronological order and nothing else.</b> No
/// ranking, no personalisation beyond visibility filtering.
///
/// <para>Note there is no <c>UserId</c> parameter - the reader is taken from
/// <c>ICurrentUser</c>. If the client could name the user whose feed to fetch, this
/// endpoint would happily serve one student another student's private feed.</para>
/// </remarks>
public sealed record GetFeedQuery(string? Cursor = null, int Limit = 20)
    : IRequest<CursorPage<PostDto>>;
