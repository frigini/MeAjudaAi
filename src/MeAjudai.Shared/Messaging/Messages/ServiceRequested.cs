using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages;

public record ServiceRequested(
    Guid RequestId,
    Guid CustomerId,
    string ServiceType,
    string Region,
    string Description,
    DateTime RequestedAt
) : IntegrationEvent("Customer");