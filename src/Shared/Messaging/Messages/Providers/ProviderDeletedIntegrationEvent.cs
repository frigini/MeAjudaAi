using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando um prestador de serviços é excluído do sistema.
/// </summary>
/// <remarks>
/// Este evento é publicado para comunicação entre módulos quando um prestador é excluído (soft delete).
/// Outros módulos podem usar este evento para:
/// - Cancelar serviços associados
/// - Enviar notificações de encerramento
/// - Arquivar dados relacionados
/// - Atualizar estatísticas
/// </remarks>
public sealed record ProviderDeletedIntegrationEvent(
    string Source,
    Guid ProviderId,
    Guid UserId,
    string Name,
    string Reason,
    DateTime DeletedAt,
    string? DeletedBy = null
) : IntegrationEvent(Source);