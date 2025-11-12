using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um prestador de serviços é ativado após verificação bem-sucedida.
/// </summary>
/// <remarks>
/// Este evento é publicado quando o prestador completa todas as etapas do processo de registro,
/// incluindo a verificação de documentos, e é ativado no sistema. Pode ser usado para notificar
/// o prestador que ele pode começar a oferecer serviços, atualizar índices de busca, ou iniciar
/// processos de onboarding.
/// </remarks>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="UserId">Identificador do usuário no Keycloak</param>
/// <param name="Name">Nome do prestador de serviços</param>
/// <param name="ActivatedBy">Identificador de quem realizou a ativação (pode ser null para ativação automática)</param>
public record ProviderActivatedDomainEvent(
    Guid AggregateId,
    int Version,
    Guid UserId,
    string Name,
    string? ActivatedBy
) : DomainEvent(AggregateId, Version);
