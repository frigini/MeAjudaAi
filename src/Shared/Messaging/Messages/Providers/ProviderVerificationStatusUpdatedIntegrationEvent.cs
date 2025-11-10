using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando o status de verificação de um prestador é atualizado.
/// </summary>
/// <remarks>
/// Este evento é publicado para comunicação entre módulos quando um prestador tem seu status alterado.
/// Outros módulos podem usar este evento para:
/// - Enviar notificações de aprovação/reprovação
/// - Atualizar permissões de acesso
/// - Sincronizar com sistemas externos
/// - Gerar relatórios de conformidade
/// </remarks>
public sealed record ProviderVerificationStatusUpdatedIntegrationEvent(
    string Source,
    Guid ProviderId,
    Guid UserId,
    string Name,
    string PreviousStatus,
    string NewStatus,
    string? UpdatedBy = null,
    string? Comments = null
) : IntegrationEvent(Source);