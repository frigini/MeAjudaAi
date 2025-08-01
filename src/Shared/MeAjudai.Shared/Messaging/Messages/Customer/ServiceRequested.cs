using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Customer;

public record ServiceRequested(
    Guid RequestId,
    Guid CustomerId,
    string ServiceType,
    string Region,
    string Description,
    DateTime RequestedAt
) : IntegrationEvent("Customer");