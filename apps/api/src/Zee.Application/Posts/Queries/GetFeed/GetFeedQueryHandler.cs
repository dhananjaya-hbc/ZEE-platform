using MediatR;
using Zee.Application.Common.Interfaces;
using Zee.Application.Common.Models;
using Zee.Application.Posts.Dtos;
using Zee.Domain.Repositories;

namespace Zee.Application.Posts.Queries.GetFeed;

/// <summary>
/// Handles <see cref="GetFeedQuery"/> by returning posts in reverse-chronological order.
/// </summary>
/// <remarks>
/// <b>This handler must not call the AI service in Phase 1.</b> <c>IAiServiceClient</c> is
/// deliberately not injected here. Feed ranking is Phase 2 work, owned by the repository
/// maintainer, and it arrives as a separate ranked path with this chronological one kept as
/// the fallback for when the AI service is slow or down. Please do not add ranking to this
/// handler - open an issue instead.
/// </remarks>
///
/// TODO: Implement Handle.
/// Acceptance criteria:
///   1. Read userId and universityId from _currentUser; throw ForbiddenAccessException if
///      either is null.
///   2. Decode request.Cursor with CursorCodec.TryDecode. A cursor that fails to decode
///      should start from the newest post rather than erroring - a stale bookmark is not
///      worth a broken feed.
///   3. Fetch limit + 1 rows from _posts.GetChronologicalFeedAsync. The extra row is how
///      you know whether another page exists WITHOUT running a second COUNT query.
///   4. If more than `limit` rows came back, drop the extra and build a nextCursor from the
///      last kept post (its CreatedAt and Id). Otherwise nextCursor is null.
///   5. Map with PostDto.FromEntity and return a CursorPage<PostDto>.
///
/// Visibility filtering (Global / University / Group-membership) belongs in the repository
/// query, not here - it has to happen in SQL to page correctly. Filtering after the fact
/// would return short pages and eventually empty ones while posts still exist.
public sealed class GetFeedQueryHandler(
    IPostRepository posts,
    ICurrentUser currentUser)
    : IRequestHandler<GetFeedQuery, CursorPage<PostDto>>
{
    private readonly IPostRepository _posts = posts;
    private readonly ICurrentUser _currentUser = currentUser;

    public Task<CursorPage<PostDto>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
