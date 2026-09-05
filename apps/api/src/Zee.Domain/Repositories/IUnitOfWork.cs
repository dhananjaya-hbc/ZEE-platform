namespace Zee.Domain.Repositories;

/// <summary>
/// Commits everything the repositories have staged, as one transaction.
/// </summary>
/// <remarks>
/// The repositories deliberately have no <c>SaveAsync</c> of their own. If each one could
/// commit independently, a handler that touches two aggregates could half-succeed, and you
/// would find that out in production rather than in review. Concentrating the commit here
/// makes the transaction boundary a single, visible line in the handler.
///
/// <para>Infrastructure implements this over <c>AppDbContext.SaveChangesAsync</c>; the
/// EF Core change tracker is what actually accumulates the pending work.</para>
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes and returns the number of rows affected.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
