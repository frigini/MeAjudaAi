using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

/// <summary>
/// Evento disparado quando uma assinatura expira.
/// </summary>
[ExcludeFromCodeCoverage]
public record SubscriptionExpiredDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId,
    int Version
) : DomainEvent(SubscriptionId, Version);
