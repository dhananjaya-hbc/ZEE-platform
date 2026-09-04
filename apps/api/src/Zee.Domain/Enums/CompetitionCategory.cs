namespace Zee.Domain.Enums;

/// <summary>The kind of competition being listed.</summary>
/// <remarks>Persisted by integer value: never renumber an existing member.</remarks>
public enum CompetitionCategory
{
    Hackathon = 0,
    Robotics = 1,
    CaseCompetition = 2,
    Other = 3,
}
