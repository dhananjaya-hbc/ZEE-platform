using MediatR;
using Zee.Application.Competitions.Dtos;
using Zee.Domain.Enums;

namespace Zee.Application.Competitions.Commands.CreateCompetition;

/// <summary>
/// Lists a new competition, organised by the authenticated student.
/// </summary>
/// <param name="OpenToAllCampuses">
/// When true the listing is visible platform-wide; when false it is restricted to the
/// organiser's own university.
/// </param>
/// <param name="RecruitingTeams">Whether teams are looking for members from the outset.</param>
/// <remarks>
/// The campus restriction is expressed as a boolean rather than a nullable
/// <c>UniversityId</c>. A student can only ever scope a competition to their own campus or
/// to everyone - never to some third university - so accepting an arbitrary id would create
/// an authorisation check with no legitimate use. The handler translates the flag into
/// either null or <c>ICurrentUser.UniversityId</c>.
/// </remarks>
public sealed record CreateCompetitionCommand(
    string Title,
    CompetitionCategory Category,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Description,
    bool OpenToAllCampuses = false,
    bool RecruitingTeams = false) : IRequest<CompetitionDto>;
