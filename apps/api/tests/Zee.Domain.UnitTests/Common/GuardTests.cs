using Zee.Domain.Common;

namespace Zee.Domain.UnitTests.Common;

/// <summary>
/// Tests for the Guard helpers.
/// </summary>
/// <remarks>
/// Guard is <c>internal</c>, so this project reaches it through
/// <c>[assembly: InternalsVisibleTo]</c> on Zee.Domain - see AssemblyInfo.cs there.
/// Exercising it directly is worth the access, because every entity factory depends on it
/// and a bug here would surface as a confusing failure in a dozen unrelated tests.
/// </remarks>
public sealed class GuardTests
{
    private const string Todo = "TODO: implement Guard, then remove this Skip.";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotEmpty_throws_for_blank_strings(string? value)
    {
        Should.Throw<DomainException>(() => Guard.NotEmpty(value));
    }

    [Fact]
    public void NotEmpty_returns_the_trimmed_value()
    {
        Guard.NotEmpty("  hello  ").ShouldBe("hello");
    }

    /// <summary>
    /// [CallerArgumentExpression] should put the caller's variable name in the message, so
    /// errors read "content must not be empty" with no hand-written literal to drift.
    /// </summary>
    [Fact]
    public void NotEmpty_names_the_offending_argument_in_the_message()
    {
        var content = "   ";

        var ex = Should.Throw<DomainException>(() => Guard.NotEmpty(content));

        ex.Message.ShouldContain("content");
    }

    [Fact]
    public void OptionalAtMost_returns_null_for_blank_input()
    {
        Guard.OptionalAtMost("   ", 10).ShouldBeNull();
    }

    [Fact]
    public void NotEmptyAndAtMost_throws_when_the_value_is_too_long()
    {
        Should.Throw<DomainException>(() => Guard.NotEmptyAndAtMost("abcdef", 3));
    }

    [Fact]
    public void DefinedEnum_rejects_a_value_that_is_not_a_declared_member()
    {
        Should.Throw<DomainException>(() => Guard.DefinedEnum((Zee.Domain.Enums.RsvpStatus)99));
    }

    /// <summary>
    /// These URLs are rendered as links on profile pages, so anything but http/https is a
    /// stored-XSS vector.
    /// </summary>
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("file:///etc/passwd")]
    [InlineData("/relative/path")]
    public void OptionalAbsoluteUrl_rejects_non_http_schemes(string url)
    {
        Should.Throw<DomainException>(() => Guard.OptionalAbsoluteUrl(url));
    }

    [Theory]
    [InlineData("https://example.com/proof")]
    [InlineData("http://example.com")]
    public void OptionalAbsoluteUrl_accepts_http_and_https(string url)
    {
        Guard.OptionalAbsoluteUrl(url).ShouldBe(url);
    }

    [Fact]
    public void EndAfterStart_throws_when_the_end_is_not_after_the_start()
    {
        var now = DateTimeOffset.UtcNow;

        Should.Throw<DomainException>(() => Guard.EndAfterStart(now, now, "An event"));
    }
}
