namespace Oficina.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public override bool Equals(object? obj) =>
        obj is Entity other && GetType() == other.GetType() && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? a, Entity? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}
