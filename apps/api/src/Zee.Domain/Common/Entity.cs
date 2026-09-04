namespace Zee.Domain.Common;

/// <summary>
/// Base class for every persisted entity in the domain.
/// </summary>
/// <remarks>
/// Identity is a <see cref="Guid"/> generated with <c>Guid.CreateVersion7()</c> rather
/// than <c>Guid.NewGuid()</c>. Version 7 GUIDs embed a millisecond timestamp in their
/// leading bits, so they sort roughly in creation order. That matters for PostgreSQL:
/// random v4 GUIDs scatter inserts across the whole primary-key B-tree and fragment it,
/// while time-ordered keys append to the right-hand edge. Same 128-bit opaque id from
/// the client's point of view, materially better write behaviour on the feed tables.
/// </remarks>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>Primary key. Assigned once, at construction.</summary>
    public Guid Id { get; protected set; }

    protected Entity(Guid id) => Id = id;

    /// <summary>Required by EF Core, which materialises entities without calling a factory.</summary>
    protected Entity()
    {
    }

    /// <summary>Creates a new time-ordered identifier for a freshly created entity.</summary>
    protected static Guid NewId() => Guid.CreateVersion7();

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // Two entities of different types are never equal even if their ids collide,
        // and transient entities (Id still default) are only equal by reference.
        return GetType() == other.GetType() && Id != Guid.Empty && Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
