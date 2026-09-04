using Zee.Domain.Common;
using Zee.Domain.Entities;

namespace Zee.Domain.UnitTests.Entities;

/// <summary>
/// Invariants of <see cref="University"/>, including the email-domain matching that gates
/// access to the entire platform.
/// </summary>
///
/// To pick this up: implement University, then remove the Skip arguments below.
public sealed class UniversityTests
{
    private const string Todo = "TODO: implement University, then remove this Skip.";

    private static University Mit() => University.Create("MIT", "US", ["mit.edu"]);

    [Fact(Skip = Todo)]
    public void Create_requires_at_least_one_verified_domain()
    {
        Should.Throw<DomainException>(() => University.Create("MIT", "US", []));
    }

    [Fact(Skip = Todo)]
    public void Create_requires_a_two_letter_country_code()
    {
        Should.Throw<DomainException>(() => University.Create("MIT", "USA", ["mit.edu"]));
    }

    [Theory(Skip = Todo)]
    [InlineData("@MIT.EDU")]
    [InlineData("MIT.EDU")]
    [InlineData("  mit.edu  ")]
    public void AddVerifiedDomain_normalises_input(string input)
    {
        var university = University.Create("MIT", "US", [input]);

        university.VerifiedEmailDomains.ShouldContain("mit.edu");
    }

    [Fact(Skip = Todo)]
    public void AcceptsEmail_accepts_an_address_at_a_verified_domain()
    {
        Mit().AcceptsEmail("ada@mit.edu").ShouldBeTrue();
    }

    /// <summary>
    /// The regression test that matters most in this file.
    /// </summary>
    /// <remarks>
    /// A suffix check (<c>EndsWith("mit.edu")</c>) passes every one of these, which would let
    /// anyone able to register a lookalike domain join MIT's campus network. Domain matching
    /// must be exact.
    /// </remarks>
    [Theory(Skip = Todo)]
    [InlineData("attacker@notmit.edu")]
    [InlineData("attacker@evilmit.edu")]
    [InlineData("attacker@mit.edu.evil.com")]
    [InlineData("attacker@sub.mit.edu")]
    public void AcceptsEmail_rejects_lookalike_and_unlisted_subdomains(string email)
    {
        Mit().AcceptsEmail(email).ShouldBeFalse();
    }

    [Theory(Skip = Todo)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-at-sign")]
    [InlineData("trailing@")]
    public void AcceptsEmail_returns_false_for_malformed_input(string email)
    {
        Mit().AcceptsEmail(email).ShouldBeFalse();
    }

    [Fact(Skip = Todo)]
    public void RemoveVerifiedDomain_refuses_to_remove_the_last_domain()
    {
        Should.Throw<DomainException>(() => Mit().RemoveVerifiedDomain("mit.edu"));
    }
}
