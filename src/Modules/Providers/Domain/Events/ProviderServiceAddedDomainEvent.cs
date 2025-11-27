using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um serviço é adicionado à lista de serviços do prestador.
/// </summary>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="ServiceId">Identificador do serviço adicionado (referência ao catálogo)</param>
public record ProviderServiceAddedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid ServiceId
) : DomainEvent(AggregateId, Version);
