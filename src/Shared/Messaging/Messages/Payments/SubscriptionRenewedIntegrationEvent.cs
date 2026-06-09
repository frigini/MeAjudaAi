using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Payments;

/// <summary>
/// Evento de integração disparado quando uma assinatura é renovada.
/// </summary>
[ExcludeFromCodeCoverage]
[CriticalEvent]
public record SubscriptionRenewedIntegrationEvent(
    string Source,
    Guid SubscriptionId,
    Guid UserId,
    DateTime NewExpiresAt
) : IntegrationEvent(Source);
