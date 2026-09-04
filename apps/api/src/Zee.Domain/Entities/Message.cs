using Zee.Domain.Common;

namespace Zee.Domain.Entities;

/// <summary>A direct message between two students.</summary>
/// <remarks>
/// Modelled as a flat sender/receiver pair rather than a Conversation aggregate. That is
/// the right call for Phase 1 - one-to-one DMs need nothing more, and a conversation list
/// is a group-by over this table. Group chat would justify revisiting it.
///
/// <para><see cref="ConversationKey"/> exists so that lookup does not need an
/// <c>OR</c> across two columns: both directions of a conversation produce the same key,
/// which indexes cleanly.</para>
/// </remarks>
public sealed class Message : Entity
{
    public const int MaxContentLength = 4000;

    private Message()
    {
    }

    private Message(Guid id, Guid senderId, Guid receiverId, string content)
        : base(id)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        ConversationKey = BuildConversationKey(senderId, receiverId);
        SentAt = DateTimeOffset.UtcNow;
    }

    public Guid SenderId { get; private set; }

    public Guid ReceiverId { get; private set; }

    public string Content { get; private set; } = null!;

    public DateTimeOffset SentAt { get; private set; }

    /// <summary>Set when the recipient opens the thread; null while unread.</summary>
    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>
    /// Direction-independent identifier for the pair, formed by ordering the two ids.
    /// Both "A to B" and "B to A" yield the same value.
    /// </summary>
    public string ConversationKey { get; private set; } = null!;

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Sender { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Receiver { get; private set; }

    /// <exception cref="DomainException">
    /// If the content is empty or too long, or the sender and receiver are the same student.
    /// </exception>
    public static Message Create(Guid senderId, Guid receiverId, string content)
    {
        Guard.NotEmpty(senderId);
        Guard.NotEmpty(receiverId);

        if (senderId == receiverId)
        {
            throw new DomainException("A message cannot be sent to yourself.");
        }

        return new Message(NewId(), senderId, receiverId, Guard.NotEmptyAndAtMost(content, MaxContentLength));
    }

    /// <summary>Marks the message read. Calling it again does not move the timestamp.</summary>
    public void MarkRead() => ReadAt ??= DateTimeOffset.UtcNow;

    /// <summary>Builds the direction-independent conversation key for two students.</summary>
    public static string BuildConversationKey(Guid a, Guid b) =>
        a.CompareTo(b) <= 0 ? $"{a:N}:{b:N}" : $"{b:N}:{a:N}";
}
