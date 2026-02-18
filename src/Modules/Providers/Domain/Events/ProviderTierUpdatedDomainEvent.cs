using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Providers.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando o tier de um prestador é atualizado.
/// </summary>
/// <remarks>
/// Normalmente disparado via webhook do Stripe após confirmação de pagamento
/// de um plano Silver, Gold ou Platinum. Também pode ser disparado ao rebaixar
/// para Standard (cancelamento de assinatura).
/// </remarks>
public sealed record ProviderTierUpdatedDomainEvent(
    Guid ProviderId,
    int Version,
    Guid UserId,
    EProviderTier PreviousTier,
    EProviderTier NewTier,
    string? UpdatedBy) : DomainEvent(ProviderId, Version);
