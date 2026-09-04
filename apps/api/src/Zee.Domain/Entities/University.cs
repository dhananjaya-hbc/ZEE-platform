using Zee.Domain.Common;

namespace Zee.Domain.Entities;

/// <summary>
/// An institution onboarded onto ZEE, together with the email domains that prove
/// membership of it.
/// </summary>
/// <remarks>
/// This entity is the platform's trust anchor. A signup is accepted only if the email
/// domain matches a row here, so "which universities exist" is deliberately data rather
/// than configuration - ZEE spans many campuses and nothing about it is hardcoded to one.
///
/// <para>Onboarding is manual and owner-reviewed in Phase 1: rows are inserted
/// deliberately, and there is no self-serve "add my university" endpoint. That is the
/// single choke point protecting the whole network from someone registering
/// <c>gmail.com</c> as a campus.</para>
/// </remarks>
public sealed class University : Entity
{
    private readonly List<string> _verifiedEmailDomains = [];

    /// <summary>Required by EF Core.</summary>
    private University()
    {
    }

    private University(Guid id, string name, string country)
        : base(id)
    {
        Name = name;
        Country = country;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Display name, e.g. "Massachusetts Institute of Technology".</summary>
    public string Name { get; private set; } = null!;

    /// <summary>ISO 3166-1 alpha-2 country code, uppercased, e.g. "US".</summary>
    public string Country { get; private set; } = null!;

    /// <summary>
    /// Lowercased bare domains that prove membership, e.g. <c>["mit.edu", "alum.mit.edu"]</c>.
    /// Stored as a PostgreSQL <c>text[]</c>.
    /// </summary>
    public IReadOnlyCollection<string> VerifiedEmailDomains => _verifiedEmailDomains.AsReadOnly();

    /// <summary>
    /// Whether this campus may currently be used to sign in. Set false to suspend a
    /// university without deleting its students' content.
    /// </summary>
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Onboards a university with at least one verified email domain.
    /// </summary>
    /// <exception cref="DomainException">
    /// If the name or country is blank, the country is not a two-letter code, or no valid
    /// domain was supplied.
    /// </exception>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Name trimmed, required, max 200 chars.
    ///   - Country uppercased and required to be exactly 2 letters (ISO 3166-1 alpha-2).
    ///   - Every supplied domain is normalised and added; at least one must survive,
    ///     otherwise throw ("A university must have at least one verified email domain").
    ///   - IsActive starts true.
    public static University Create(string name, string country, IEnumerable<string> verifiedEmailDomains)
        => throw new NotImplementedException();

    /// <summary>
    /// Adds a domain to the allowlist. Must accept "@mit.edu", "MIT.EDU" or "mit.edu" and
    /// store the normalised form. Adding a domain already present is a no-op.
    /// </summary>
    ///
    /// TODO: Implement, along with the private NormaliseDomain helper below.
    public void AddVerifiedDomain(string domain)
        => throw new NotImplementedException();

    /// <summary>Removes a domain from the allowlist. The last domain cannot be removed.</summary>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Removing a domain that is not present is a no-op.
    ///   - Removing the only remaining domain throws DomainException - a university with
    ///     no domains can never authenticate anyone and is silently broken.
    public void RemoveVerifiedDomain(string domain)
        => throw new NotImplementedException();

    /// <summary>Suspends sign-in for this campus. Existing content is untouched.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Re-enables sign-in for this campus.</summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Whether the given address belongs to this university.
    /// </summary>
    /// <remarks>
    /// <b>Match the domain exactly, never by suffix.</b> Suffix matching is a serious
    /// security hole: <c>notmit.edu</c> ends with <c>mit.edu</c> under a naive
    /// <c>EndsWith</c> check, which would let anyone who can register a lookalike domain
    /// join a campus. Subdomains must be listed explicitly instead.
    /// </remarks>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Split on the LAST '@'; return false for null/blank/malformed input.
    ///   - Compare the lowercased domain for exact equality against the allowlist.
    ///   - Regression test required: a university with "mit.edu" must REJECT
    ///     "someone@notmit.edu" and "someone@mit.edu.evil.com".
    public bool AcceptsEmail(string email)
        => throw new NotImplementedException();

    /// <summary>
    /// Normalises a domain for storage: trims, strips a leading '@', lowercases, and
    /// rejects anything that is not a plausible domain.
    /// </summary>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Rejects values with no dot, a leading/trailing dot, whitespace, or an '@'.
    ///   - Max length 253 (the DNS limit).
    private static string NormaliseDomain(string domain)
        => throw new NotImplementedException();
}
