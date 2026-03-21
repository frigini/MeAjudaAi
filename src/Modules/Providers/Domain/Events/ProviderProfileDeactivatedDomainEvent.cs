using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um perfil de prestador de serviços é desativado.
/// </summary>
public record ProviderProfileDeactivatedDomainEvent(
    Guid AggregateId,
    int Version,
    string? UpdatedBy) : DomainEvent(AggregateId, Version);
