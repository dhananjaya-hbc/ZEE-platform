namespace Zee.Domain.Enums;

/// <summary>Who is allowed to see a post.</summary>
/// <remarks>
/// Values are persisted by their integer value, so existing members must never be
/// renumbered. Append new members at the end.
/// </remarks>
public enum PostVisibility
{
    /// <summary>Visible to every verified student on ZEE, on any campus.</summary>
    Global = 0,

    /// <summary>Visible only to students whose <c>UniversityId</c> matches the author's.</summary>
    University = 1,

    /// <summary>Visible only to members of the post's group. Requires <c>GroupId</c> to be set.</summary>
    Group = 2,
}
