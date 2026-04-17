using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Payments.Domain.Events;

/// <summary>
/// Evento disparado quando uma assinatura é ativada.
/// </summary>
[ExcludeFromCodeCoverage]
public record SubscriptionActivatedDomainEvent(
    Guid SubscriptionId, 
    Guid ProviderId, 
    string ExternalSubscriptionId,
    int Version
) : DomainEvent(SubscriptionId, Version);
