using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Application.Competitions.Dtos;

/// <summary>What the API returns for a competition listing.</summary>
/// <param name="UniversityId">Null when the competition is open to students on any campus.</param>
/// <param name="IsOpenToAllCampuses">
/// Convenience flag mirroring <paramref name="UniversityId"/> being null, so clients do not
/// each reinvent that rule.
/// </param>
public sealed record CompetitionDto(
    Guid Id,
    string Title,
    CompetitionCategory Category,
    Guid OrganizerId,
    string OrganizerName,
    Guid? UniversityId,
    bool IsOpenToAllCampuses,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Description,
    bool RecruitingTeams)
{
    /// <summary>Projects an entity to its DTO.</summary>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - IsOpenToAllCampuses == (entity.UniversityId is null).
    ///   - OrganizerName falls back to "Unknown" when the Organizer navigation is not loaded.
    public static CompetitionDto FromEntity(Competition competition)
        => throw new NotImplementedException();
}
