using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um prestador de serviços completa as informações básicas
/// e entra na etapa de verificação de documentos.
/// </summary>
/// <remarks>
/// Este evento é publicado quando o prestador transita do status PendingBasicInfo para
/// PendingDocumentVerification. Pode ser usado para notificar o prestador sobre os próximos passos,
/// enviar instruções sobre upload de documentos, ou iniciar processos de verificação assíncronos.
/// </remarks>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="UserId">Identificador do usuário no Keycloak</param>
/// <param name="Name">Nome do prestador de serviços</param>
/// <param name="UpdatedBy">Identificador de quem realizou a atualização (pode ser null para auto-atualização)</param>
public record ProviderAwaitingVerificationDomainEvent(
    Guid AggregateId,
    int Version,
    Guid UserId,
    string Name,
    string? UpdatedBy
) : DomainEvent(AggregateId, Version);
