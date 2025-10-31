using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um documento é removido do prestador de serviços.
/// </summary>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="DocumentType">Tipo do documento removido</param>
/// <param name="DocumentNumber">Número do documento</param>
public record ProviderDocumentRemovedDomainEvent(
    Guid AggregateId,
    int Version,
    EDocumentType DocumentType,
    string DocumentNumber
) : DomainEvent(AggregateId, Version);