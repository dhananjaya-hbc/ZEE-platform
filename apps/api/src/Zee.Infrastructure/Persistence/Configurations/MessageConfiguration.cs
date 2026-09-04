using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Message"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("messages"). Key: Id.
///   - Content: required, max Message.MaxContentLength.
///   - ConversationKey: required, max 80 (two 32-char hex GUIDs plus a separator).
///   - Index on (ConversationKey, SentAt DESC) - this is the index that makes opening a
///     thread a single indexed seek instead of an OR across sender and receiver.
///   - Index on (ReceiverId, ReadAt) for unread counts.
///   - Sender and Receiver: many-to-one on SenderId / ReceiverId,
///     OnDelete(DeleteBehavior.Restrict) on BOTH. Two cascade paths into the same table is
///     something PostgreSQL will accept but which makes deletion order surprising; restrict
///     and handle account deletion explicitly.
public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
