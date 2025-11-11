using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando um novo prestador de serviços é registrado no sistema.
/// </summary>
/// <remarks>
/// Este evento é publicado para comunicação entre módulos quando um prestador é criado.
/// Outros módulos podem usar este evento para:
/// - Criar perfis associados
/// - Enviar notificações de boas-vindas
/// - Sincronizar dados com sistemas externos
/// - Atualizar estatísticas e métricas
/// </remarks>
public sealed record ProviderRegisteredIntegrationEvent(
    string Source,
    Guid ProviderId,
    Guid UserId,
    string Name,
    string ProviderType,
    string Email,
    string? PhoneNumber = null,
    string? City = null,
    string? State = null,
    DateTime? RegisteredAt = null
) : IntegrationEvent(Source);