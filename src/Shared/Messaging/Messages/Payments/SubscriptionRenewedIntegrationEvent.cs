using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Payments;

/// <summary>
/// Evento de integração disparado quando uma assinatura é renovada.
/// </summary>
[ExcludeFromCodeCoverage]
public record SubscriptionRenewedIntegrationEvent(
    string Source,
    Guid SubscriptionId,
    Guid UserId
) : IntegrationEvent(Source);
