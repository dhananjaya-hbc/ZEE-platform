using Microsoft.EntityFrameworkCore;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IUniversityRepository"/>.</summary>
public sealed class UniversityRepository(AppDbContext db) : IUniversityRepository
{
    private readonly AppDbContext _db = db;

    /// TODO: Implement.
    public Task<University?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement. SECURITY-SENSITIVE - this is the gate on the whole platform.
    ///
    /// Acceptance criteria:
    ///   - Extract the domain after the LAST '@', trimmed and lowercased. Return null for
    ///     malformed input rather than throwing.
    ///   - Match with an EXACT array-containment query against VerifiedEmailDomains
    ///     (Npgsql translates List.Contains to the indexed PostgreSQL array operator).
    ///     Never use EndsWith or LIKE '%domain' - "notmit.edu" would match "mit.edu" and
    ///     anyone able to register a lookalike domain could join that campus.
    ///   - Filter to IsActive == true, so suspending a university actually blocks sign-in.
    ///   - Tests must include the notmit.edu regression case.
    public async Task<University?> FindByEmailDomainAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var atIndex = email.LastIndexOf('@');

        if (atIndex < 0 || atIndex == email.Length - 1)
        {
            return null;
        }

        var domain = email[(atIndex + 1)..].Trim().ToLowerInvariant();

        return await _db.Universities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.IsActive && u.VerifiedEmailDomains.Contains(domain),
                cancellationToken);
    }
    /// TODO: Implement - active universities, ordered by name.
    public Task<IReadOnlyList<University>> GetActiveAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Add(University university) => _db.Universities.Add(university);
}
