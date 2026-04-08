using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Documents;

/// <summary>
/// Evento de integração disparado quando um documento é verificado com sucesso.
/// </summary>
/// <remarks>
/// Este evento é publicado para comunicação entre módulos quando um documento
/// completa o processo de verificação via OCR/Document Intelligence.
/// Outros módulos podem usar este evento para:
/// - Atualizar status de verificação do prestador
/// - Liberar próximas etapas no fluxo de onboarding
/// - Enviar notificações de aprovação
/// - Atualizar dashboards e métricas
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed record DocumentVerifiedIntegrationEvent(
    string Source,
    Guid DocumentId,
    Guid ProviderId,
    string DocumentType,
    bool HasOcrData,
    DateTime? VerifiedAt = null
) : IntegrationEvent(Source);
