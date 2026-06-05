using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Payments;

/// <summary>
/// Evento de integração disparado quando uma assinatura é ativada.
/// </summary>
[ExcludeFromCodeCoverage]
public record SubscriptionActivatedIntegrationEvent(
    string Source,
    Guid SubscriptionId,
    Guid UserId
) : IntegrationEvent(Source);
