using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

/// <summary>
/// Evento disparado quando uma assinatura é renovada.
/// </summary>
[ExcludeFromCodeCoverage]
public record SubscriptionRenewedDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId, 
    DateTime NewExpiresAt,
    int Version
) : DomainEvent(SubscriptionId, Version);
