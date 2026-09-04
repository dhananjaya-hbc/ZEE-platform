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

    /// <exception cref="DomainException">
    /// If the title is blank, the date is in the future, or the proof URL is not an
    /// absolute http/https URL.
    /// </exception>
    public static Achievement Create(
        Guid userId,
        string title,
        DateTimeOffset date,
        string? description = null,
        Guid? competitionId = null,
        string? proofUrl = null)
    {
        Guard.NotEmpty(userId);

        // A small clock-skew allowance keeps clients with slightly fast clocks from being
        // rejected for an achievement dated "now".
        if (date > DateTimeOffset.UtcNow.AddDays(1))
        {
            throw new DomainException("An achievement cannot be dated in the future.");
        }

        if (competitionId is not null)
        {
            Guard.NotEmpty(competitionId.Value, nameof(competitionId));
        }

        return new Achievement(
            NewId(),
            userId,
            competitionId,
            Guard.NotEmptyAndAtMost(title, 200),
            Guard.OptionalAtMost(description, 2000),
            date.ToUniversalTime(),
            Guard.OptionalAbsoluteUrl(proofUrl));
    }
}
