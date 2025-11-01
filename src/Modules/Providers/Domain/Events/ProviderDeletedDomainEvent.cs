using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um prestador de serviços é excluído logicamente.
/// </summary>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="Name">Nome do prestador de serviços excluído</param>
/// <param name="DeletedBy">Quem fez a exclusão</param>
public record ProviderDeletedDomainEvent(
    Guid AggregateId,
    int Version,
    string Name,
    string? DeletedBy
) : DomainEvent(AggregateId, Version);
