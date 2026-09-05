using Zee.Domain.Common;
using Zee.Domain.Entities;

namespace Zee.Domain.UnitTests.Entities;

/// <summary>Invariants of <see cref="Message"/>.</summary>
public sealed class MessageTests
{
    private const string Todo = "TODO: implement Message, then remove this Skip.";

    private static readonly Guid Alice = Guid.CreateVersion7();
    private static readonly Guid Bob = Guid.CreateVersion7();

    [Fact(Skip = Todo)]
    public void Create_throws_when_sender_and_receiver_are_the_same()
    {
        Should.Throw<DomainException>(() => Message.Create(Alice, Alice, "hello"));
    }

    [Fact(Skip = Todo)]
    public void Create_throws_when_the_content_is_blank()
    {
        Should.Throw<DomainException>(() => Message.Create(Alice, Bob, "   "));
    }

    /// <summary>
    /// The property the whole conversation index depends on: both directions of a thread
    /// must produce the same key, or a conversation splits into two halves.
    /// </summary>
    [Fact(Skip = Todo)]
    public void BuildConversationKey_is_symmetric()
    {
        Message.BuildConversationKey(Alice, Bob)
            .ShouldBe(Message.BuildConversationKey(Bob, Alice));
    }

    [Fact(Skip = Todo)]
    public void MarkRead_is_idempotent()
    {
        var message = Message.Create(Alice, Bob, "hello");

        message.MarkRead();
        var first = message.ReadAt;
        message.MarkRead();

        message.ReadAt.ShouldBe(first);
    }
}
