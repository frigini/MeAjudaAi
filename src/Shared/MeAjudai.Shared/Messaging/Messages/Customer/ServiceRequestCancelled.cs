using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Customer;

public record ServiceRequestCancelled(
    Guid RequestId,
    Guid CustomerId,
    string CancellationReason,
    DateTime CancelledAt
) : IntegrationEvent("Customer");