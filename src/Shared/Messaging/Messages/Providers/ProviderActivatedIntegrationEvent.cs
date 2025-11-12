using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando um prestador é ativado no sistema.
/// </summary>
/// <remarks>
/// Este evento é publicado para comunicação entre módulos quando um prestador
/// completa todas as etapas de verificação e é ativado.
/// Outros módulos podem usar este evento para:
/// - Adicionar o prestador aos índices de busca
/// - Enviar notificações de ativação
/// - Habilitar recursos específicos para prestadores ativos
/// - Atualizar dashboards e métricas
/// </remarks>
public sealed record ProviderActivatedIntegrationEvent(
    string Source,
    Guid ProviderId,
    Guid UserId,
    string Name,
    string? ActivatedBy = null,
    DateTime? ActivatedAt = null
) : IntegrationEvent(Source);
