using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Events;

/// <summary>
/// NOTA: IntegrationEvent usa UuidGenerator.NewId() e DateTime.UtcNow diretamente pois eventos
/// são criados no momento da publicação e não precisam de injeção de dependência para testabilidade.
/// O Id (UUID v7 baseado em tempo) e timestamp representam o momento exato da criação do evento.
/// </summary>
public abstract record IntegrationEvent(
    string Source
) : IIntegrationEvent
{
    public Guid Id { get; } = UuidGenerator.NewId();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public string Version { get; init; } = "1.0";
}
