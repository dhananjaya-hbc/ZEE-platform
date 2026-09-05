namespace Zee.Domain.Enums;

/// <summary>A student's stated intent to attend an event.</summary>
/// <remarks>
/// There is no "not going" member on purpose. Withdrawing removes the RSVP row rather than
/// recording a negative, which keeps attendance counts a simple filtered count and avoids
/// storing a signal nobody asked to publish.
///
/// <para>Persisted by integer value: never renumber an existing member.</para>
/// </remarks>
public enum RsvpStatus
{
    /// <summary>Committed to attending.</summary>
    Going = 0,

    /// <summary>Watching the event, not committed.</summary>
    Interested = 1,
}
