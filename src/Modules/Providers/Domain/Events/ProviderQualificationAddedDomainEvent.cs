using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando uma qualificação é adicionada ao prestador de serviços.
/// </summary>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="QualificationName">Nome da qualificação</param>
/// <param name="IssuingOrganization">Organização emissora</param>
public record ProviderQualificationAddedDomainEvent(
    Guid AggregateId,
    int Version,
    string QualificationName,
    string? IssuingOrganization
) : DomainEvent(AggregateId, Version);
