using MeAjudai.Shared.Events;

namespace MeAjudai.Shared.Messaging.Messages;

public record ServiceRequested(
    Guid RequestId,
    Guid CustomerId,
    string ServiceType,
    string Region,
    string Description,
    DateTime RequestedAt
) : IntegrationEvent("Customer");