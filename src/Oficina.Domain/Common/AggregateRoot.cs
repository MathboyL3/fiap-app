namespace Oficina.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();

    protected void Raise(IDomainEvent evt) => _events.Add(evt);
    public void ClearEvents() => _events.Clear();
}

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
