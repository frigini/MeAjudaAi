using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando o perfil de um prestador de serviços é atualizado.
/// </summary>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="Name">Novo nome do prestador de serviços</param>
/// <param name="Email">Novo email de contato</param>
/// <param name="UpdatedBy">Quem fez a atualização</param>
public record ProviderProfileUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    string Name,
    string Email,
    string? UpdatedBy
) : DomainEvent(AggregateId, Version);