using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = UuidGenerator.NewId();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
