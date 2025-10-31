using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um novo prestador de serviços é registrado no sistema.
/// </summary>
/// <remarks>
/// Este evento é publicado automaticamente quando um prestador de serviços é criado através do construtor
/// da entidade Provider. Pode ser usado para integração com outros bounded contexts,
/// envio de emails de boas-vindas, criação de perfis em outros sistemas, etc.
/// </remarks>
/// <param name="AggregateId">Identificador único do prestador de serviços registrado</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="UserId">Identificador do usuário no Keycloak</param>
/// <param name="Name">Nome do prestador de serviços</param>
/// <param name="Type">Tipo do prestador de serviços</param>
/// <param name="Email">Email de contato do prestador de serviços</param>
public record ProviderRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    Guid UserId,
    string Name,
    EProviderType Type,
    string Email
) : DomainEvent(AggregateId, Version);