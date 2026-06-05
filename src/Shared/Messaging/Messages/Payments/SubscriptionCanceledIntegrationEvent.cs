using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Payments;

/// <summary>
/// Evento de integração disparado quando uma assinatura é cancelada.
/// </summary>
[ExcludeFromCodeCoverage]
public record SubscriptionCanceledIntegrationEvent(
    string Source,
    Guid SubscriptionId,
    Guid UserId
) : IntegrationEvent(Source);
