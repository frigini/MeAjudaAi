using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Domain;

/// <summary>
/// Interface para entidades que possuem eventos de domínio.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
