using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Domain;

/// <summary>
/// NOTA: BaseEntity usa DateTime.UtcNow diretamente para CreatedAt e UpdatedAt.
/// Isto é intencional pois:
/// 1. Timestamps de auditoria devem refletir o momento exato da persistência
/// 2. A alternativa seria injetar IDateTimeProvider em todas as entidades (anti-pattern)
/// 3. Para testes, use builders que permitem setar as datas explicitamente
/// </summary>
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
