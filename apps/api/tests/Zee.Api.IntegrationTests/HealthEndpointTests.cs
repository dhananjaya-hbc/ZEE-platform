using System.Net;
using Zee.Api.IntegrationTests.Infrastructure;

namespace Zee.Api.IntegrationTests;

/// <summary>
/// End-to-end checks that the host boots and the pipeline behaves.
/// </summary>
/// <remarks>
/// These are NOT skipped. They pass today and are the scaffolding's smoke test: if the DI
/// container cannot be built - a missing registration, a bad options binding - these fail
/// immediately and point at it, which is much easier to diagnose than a runtime error in a
/// container.
/// </remarks>
public sealed class HealthEndpointTests(ZeeApiFactory factory) : IClassFixture<ZeeApiFactory>
{
    private readonly ZeeApiFactory _factory = factory;

    [Fact]
    public async Task Health_endpoint_is_reachable_anonymously()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Confirms [Authorize] on ApiControllerBase is actually in force. If this ever starts
    /// returning 200, the whole API has been quietly made public.
    /// </summary>
    [Fact]
    public async Task Feed_endpoint_requires_authentication()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/feed", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// TODO: Once ZeeApiFactory.CreateAccessTokenFor is implemented, assert that an
    /// authenticated GET /api/feed returns 501 while GetFeedQueryHandler is a stub, and
    /// 200 with a page of posts once it is implemented.
    [Fact(Skip = "TODO: needs ZeeApiFactory.CreateAccessTokenFor.")]
    public void Authenticated_feed_request_reaches_the_handler()
    {
    }
}
