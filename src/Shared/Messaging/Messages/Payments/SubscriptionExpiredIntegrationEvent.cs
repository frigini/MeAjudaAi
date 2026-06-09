using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Payments;

/// <summary>
/// Evento de integração disparado quando uma assinatura expira.
/// </summary>
[ExcludeFromCodeCoverage]
[CriticalEvent]
public record SubscriptionExpiredIntegrationEvent(
    string Source,
    Guid SubscriptionId,
    Guid UserId
) : IntegrationEvent(Source);
