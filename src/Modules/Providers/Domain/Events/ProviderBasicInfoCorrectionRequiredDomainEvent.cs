using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um prestador de serviços precisa corrigir suas informações básicas
/// durante o processo de verificação de documentos.
/// </summary>
/// <remarks>
/// Este evento é publicado quando o prestador é retornado do status PendingDocumentVerification para
/// PendingBasicInfo devido a inconsistências ou informações faltantes identificadas durante a verificação.
/// Pode ser usado para notificar o prestador sobre as correções necessárias, enviar emails de notificação,
/// ou registrar a solicitação de correção em sistemas de auditoria.
/// </remarks>
/// <param name="AggregateId">Identificador único do prestador de serviços</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="UserId">Identificador do usuário no Keycloak</param>
/// <param name="Name">Nome do prestador de serviços</param>
/// <param name="Reason">Motivo detalhado da correção necessária</param>
/// <param name="RequestedBy">Identificador de quem solicitou a correção (verificador/administrador)</param>
public record ProviderBasicInfoCorrectionRequiredDomainEvent(
    Guid AggregateId,
    int Version,
    Guid UserId,
    string Name,
    string Reason,
    string? RequestedBy
) : DomainEvent(AggregateId, Version);
