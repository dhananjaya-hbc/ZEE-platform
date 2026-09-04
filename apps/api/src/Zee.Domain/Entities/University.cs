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
    public static University Create(string name, string country, IEnumerable<string> verifiedEmailDomains)
    {
        ArgumentNullException.ThrowIfNull(verifiedEmailDomains);

        var normalisedName = Guard.NotEmptyAndAtMost(name, 200);
        var normalisedCountry = Guard.NotEmpty(country).ToUpperInvariant();

        if (normalisedCountry.Length != 2)
        {
            throw new DomainException("country must be a two-letter ISO 3166-1 alpha-2 code.");
        }

        var university = new University(NewId(), normalisedName, normalisedCountry);

        foreach (var domain in verifiedEmailDomains)
        {
            university.AddVerifiedDomain(domain);
        }

        if (university._verifiedEmailDomains.Count == 0)
        {
            throw new DomainException("A university must have at least one verified email domain.");
        }

        return university;
    }

    /// <summary>
    /// Adds a domain to the allowlist. Accepts "@mit.edu", "MIT.EDU" or "mit.edu" and
    /// stores the normalised form. Adding a domain that is already present is a no-op.
    /// </summary>
    public void AddVerifiedDomain(string domain)
    {
        var normalised = NormaliseDomain(domain);

        if (!_verifiedEmailDomains.Contains(normalised, StringComparer.Ordinal))
        {
            _verifiedEmailDomains.Add(normalised);
        }
    }

    /// <summary>Removes a domain from the allowlist. The last domain cannot be removed.</summary>
    public void RemoveVerifiedDomain(string domain)
    {
        var normalised = NormaliseDomain(domain);

        if (_verifiedEmailDomains.Count == 1 && _verifiedEmailDomains.Contains(normalised, StringComparer.Ordinal))
        {
            throw new DomainException("A university must keep at least one verified email domain.");
        }

        _verifiedEmailDomains.Remove(normalised);
    }

    /// <summary>Suspends sign-in for this campus. Existing content is untouched.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Re-enables sign-in for this campus.</summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Whether the given address belongs to this university.
    /// </summary>
    /// <remarks>
    /// Matches the domain exactly rather than by suffix. Suffix matching would be a
    /// serious hole: <c>notmit.edu</c> ends with <c>mit.edu</c> under a naive
    /// <c>EndsWith</c> check, and <c>evil.com/@mit.edu</c> style tricks rely on exactly
    /// that sort of sloppiness. Subdomains must be listed explicitly.
    /// </remarks>
    public bool AcceptsEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var atIndex = email.LastIndexOf('@');

        if (atIndex < 0 || atIndex == email.Length - 1)
        {
            return false;
        }

        var domain = email[(atIndex + 1)..].Trim().ToLowerInvariant();

        return _verifiedEmailDomains.Contains(domain, StringComparer.Ordinal);
    }

    private static string NormaliseDomain(string domain)
    {
        var normalised = Guard.NotEmptyAndAtMost(domain, 253).TrimStart('@').ToLowerInvariant();

        if (!normalised.Contains('.', StringComparison.Ordinal) ||
            normalised.StartsWith('.') ||
            normalised.EndsWith('.') ||
            normalised.Contains(' ', StringComparison.Ordinal) ||
            normalised.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException($"'{domain}' is not a valid email domain.");
        }

        return normalised;
    }
}
