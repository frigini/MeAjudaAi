using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Shared.Events;

/// <summary>
/// NOTA: DomainEvent usa DateTime.UtcNow diretamente pois eventos de domínio são criados
/// no momento da execução de uma ação de domínio e não precisam de injeção de dependência.
/// O timestamp representa o momento exato em que o evento ocorreu.
/// </summary>
public abstract record DomainEvent(
    Guid AggregateId,
    int Version
) : IDomainEvent
{
    public Guid Id { get; } = UuidGenerator.NewId();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
