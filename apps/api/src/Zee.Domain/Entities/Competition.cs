using Zee.Domain.Common;
using Zee.Domain.Enums;

namespace Zee.Domain.Entities;

/// <summary>
/// A hackathon, robotics contest, case competition or similar, listed by a student organiser.
/// </summary>
/// <remarks>
/// A null <see cref="UniversityId"/> means the competition is open to students on any
/// campus - this is one of the main reasons ZEE is global rather than a set of isolated
/// campus networks. A set <see cref="UniversityId"/> restricts it to one institution.
/// </remarks>
public sealed class Competition : Entity
{
    /// <summary>Required by EF Core.</summary>
    private Competition()
    {
    }

    private Competition(
        Guid id,
        string title,
        CompetitionCategory category,
        Guid organizerId,
        Guid? universityId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string description,
        bool recruitingTeams)
        : base(id)
    {
        Title = title;
        Category = category;
        OrganizerId = organizerId;
        UniversityId = universityId;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
        RecruitingTeams = recruitingTeams;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Title { get; private set; } = null!;

    public CompetitionCategory Category { get; private set; }

    /// <summary>The student who listed it.</summary>
    public Guid OrganizerId { get; private set; }

    /// <summary>Restricting campus, or null when open to students anywhere.</summary>
    public Guid? UniversityId { get; private set; }

    public DateTimeOffset StartDate { get; private set; }

    public DateTimeOffset EndDate { get; private set; }

    public string Description { get; private set; } = null!;

    /// <summary>Whether teams are currently looking for members. Drives the "find a team" surface.</summary>
    public bool RecruitingTeams { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Organizer { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public University? University { get; private set; }

    /// <summary>Creates a competition listing.</summary>
    /// <exception cref="DomainException">
    /// If the title or description is blank, or the competition ends before it starts.
    /// </exception>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - category must be a defined enum member; organizerId must not be Guid.Empty.
    ///   - endDate must be strictly after startDate.
    ///   - Both dates stored as UTC (call .ToUniversalTime()) so cross-campus listings from
    ///     different timezones compare correctly.
    ///   - Title required, max 200. Description required, max 10000.
    public static Competition Create(
        string title,
        CompetitionCategory category,
        Guid organizerId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string description,
        Guid? universityId = null,
        bool recruitingTeams = false)
        => throw new NotImplementedException();

    /// <summary>Opens or closes team recruiting.</summary>
    public void SetRecruitingTeams(bool recruiting) => RecruitingTeams = recruiting;

    /// <summary>Updates the editable details of a listing.</summary>
    ///
    /// TODO: Implement, reusing the same date and length rules as Create.
    public void UpdateDetails(string title, string description, DateTimeOffset startDate, DateTimeOffset endDate)
        => throw new NotImplementedException();
}
