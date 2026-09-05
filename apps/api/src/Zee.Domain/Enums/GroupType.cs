namespace Zee.Domain.Enums;

/// <summary>What kind of community a group represents.</summary>
/// <remarks>
/// This is not just a label - it determines campus scoping. Course, Club and Dorm groups
/// belong to exactly one university; GlobalInterest groups deliberately span campuses and
/// must not carry a <c>UniversityId</c>. <c>Group.Create</c> enforces that.
///
/// <para>Persisted by integer value: never renumber an existing member.</para>
/// </remarks>
public enum GroupType
{
    /// <summary>A specific class or module at one university, e.g. "MIT 6.006 Fall 2026".</summary>
    Course = 0,

    /// <summary>A student society or organisation at one university.</summary>
    Club = 1,

    /// <summary>A residence hall or dorm community at one university.</summary>
    Dorm = 2,

    /// <summary>A cross-campus interest community, e.g. "Competitive Robotics". Not campus-scoped.</summary>
    GlobalInterest = 3,
}
