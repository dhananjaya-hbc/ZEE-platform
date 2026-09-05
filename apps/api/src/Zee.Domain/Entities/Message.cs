using Zee.Domain.Common;

namespace Zee.Domain.Entities;

/// <summary>A direct message between two students.</summary>
/// <remarks>
/// Modelled as a flat sender/receiver pair rather than a Conversation aggregate. That is
/// the right call for Phase 1 - one-to-one DMs need nothing more, and a conversation list
/// is a group-by over this table. Group chat would justify revisiting it.
/// </remarks>
public sealed class Message : Entity
{
    public const int MaxContentLength = 4000;

    /// <summary>Required by EF Core.</summary>
    private Message()
    {
    }

    private Message(Guid id, Guid senderId, Guid receiverId, string content, string conversationKey)
        : base(id)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        ConversationKey = conversationKey;
        SentAt = DateTimeOffset.UtcNow;
    }

    public Guid SenderId { get; private set; }

    public Guid ReceiverId { get; private set; }

    public string Content { get; private set; } = null!;

    public DateTimeOffset SentAt { get; private set; }

    /// <summary>Set when the recipient opens the thread; null while unread.</summary>
    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>
    /// Direction-independent identifier for the pair of students in this conversation.
    /// </summary>
    /// <remarks>
    /// Exists so loading a thread is an indexed equality lookup on one column, instead of
    /// <c>WHERE (sender = A AND receiver = B) OR (sender = B AND receiver = A)</c> - which
    /// PostgreSQL struggles to serve from an index and which gets slower as the table grows.
    /// </remarks>
    public string ConversationKey { get; private set; } = null!;

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Sender { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Receiver { get; private set; }

    /// <summary>Sends a message.</summary>
    /// <exception cref="DomainException">
    /// If the content is empty or too long, or the sender and receiver are the same student.
    /// </exception>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Neither id may be Guid.Empty, and they must differ (no messaging yourself).
    ///   - Content required, trimmed, max MaxContentLength.
    ///   - ConversationKey set from BuildConversationKey.
    public static Message Create(Guid senderId, Guid receiverId, string content)
        => throw new NotImplementedException();

    /// <summary>Marks the message read. Calling it again must not move the timestamp.</summary>
    ///
    /// TODO: Implement (assign only when ReadAt is currently null).
    public void MarkRead()
        => throw new NotImplementedException();

    /// <summary>
    /// Builds the direction-independent conversation key for two students.
    /// </summary>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Order the two ids consistently (e.g. by Guid.CompareTo) before combining, so
    ///     BuildConversationKey(a, b) == BuildConversationKey(b, a). A test for exactly
    ///     that symmetry is the point of this method.
    public static string BuildConversationKey(Guid a, Guid b)
        => throw new NotImplementedException();
}
