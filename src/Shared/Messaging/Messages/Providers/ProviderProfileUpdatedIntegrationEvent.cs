using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Providers;

/// <summary>
/// Evento de integração disparado quando o perfil de um prestador é atualizado.
/// </summary>
/// <remarks>
/// Este evento é publicado para comunicação entre módulos quando dados do prestador são alterados.
/// Outros módulos podem usar este evento para:
/// - Sincronizar informações em caches
/// - Atualizar índices de busca
/// - Notificar sistemas externos
/// - Manter auditoria de mudanças
/// </remarks>
public sealed record ProviderProfileUpdatedIntegrationEvent(
    string Source,
    Guid ProviderId,
    Guid UserId,
    string Name,
    IEnumerable<string> UpdatedFields,
    string? UpdatedBy = null,
    string? PreviousName = null,
    string? NewEmail = null,
    string? NewPhoneNumber = null
) : IntegrationEvent(Source);