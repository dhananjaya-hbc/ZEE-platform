using Zee.Domain.Common;

namespace Zee.Domain.Entities;

/// <summary>
/// Something a student accomplished - a competition placing, a publication, a certification.
/// </summary>
/// <remarks>
/// <see cref="CompetitionId"/> is nullable because plenty of achievements have no ZEE
/// competition behind them. When it is set, the achievement links to a listing on the
/// platform and can be displayed alongside it.
///
/// <para>Achievements are self-reported in Phase 1. <see cref="ProofUrl"/> is a link a
/// human can check, not a verified claim, and nothing in the system treats it as one.</para>
/// </remarks>
public sealed class Achievement : Entity
{
    /// <summary>Required by EF Core.</summary>
    private Achievement()
    {
    }

    private Achievement(
        Guid id,
        Guid userId,
        Guid? competitionId,
        string title,
        string? description,
        DateTimeOffset date,
        string? proofUrl)
        : base(id)
    {
        UserId = userId;
        CompetitionId = competitionId;
        Title = title;
        Description = description;
        Date = date;
        ProofUrl = proofUrl;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>The student who earned it.</summary>
    public Guid UserId { get; private set; }

    /// <summary>The related ZEE competition, if any.</summary>
    public Guid? CompetitionId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    /// <summary>When it was earned - not when the row was created.</summary>
    public DateTimeOffset Date { get; private set; }

    /// <summary>An http/https link a reader can follow to check the claim. Optional.</summary>
    public string? ProofUrl { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? User { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public Competition? Competition { get; private set; }

    /// <summary>Records an achievement.</summary>
    /// <exception cref="DomainException">
    /// If the title is blank, the date is in the future, or the proof URL is not an
    /// absolute http/https URL.
    /// </exception>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - userId must not be Guid.Empty.
    ///   - Reject dates more than 1 day in the future. The one-day slack is intentional:
    ///     client clocks drift, and rejecting an achievement dated "today" because a phone
    ///     is four hours fast is a bad bug to ship.
    ///   - Title required, max 200. Description optional, max 2000.
    ///   - ProofUrl optional, but if present must be an absolute http/https URL
    ///     (Guard.OptionalAbsoluteUrl) - these get rendered as links.
    ///   - Date stored as UTC.
    public static Achievement Create(
        Guid userId,
        string title,
        DateTimeOffset date,
        string? description = null,
        Guid? competitionId = null,
        string? proofUrl = null)
        => throw new NotImplementedException();
}
