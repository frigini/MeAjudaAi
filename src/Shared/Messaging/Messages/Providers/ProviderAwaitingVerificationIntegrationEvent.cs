using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando um prestador completa informações básicas
/// e entra na etapa de verificação de documentos.
/// </summary>
/// <remarks>
/// Este evento é publicado para comunicação entre módulos quando um prestador
/// avança do status PendingBasicInfo para PendingDocumentVerification.
/// Outros módulos podem usar este evento para:
/// - Enviar notificações sobre próximos passos
/// - Preparar processos de verificação
/// - Atualizar dashboards e métricas
/// </remarks>
public sealed record ProviderAwaitingVerificationIntegrationEvent(
    string Source,
    Guid ProviderId,
    Guid UserId,
    string Name,
    string? UpdatedBy = null,
    DateTime? TransitionedAt = null
) : IntegrationEvent(Source);
