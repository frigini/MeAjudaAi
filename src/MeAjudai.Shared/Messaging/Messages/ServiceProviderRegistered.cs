using MeAjudai.Shared.Events;

namespace MeAjudai.Shared.Messaging.Messages;

public record ServiceProviderRegistered(
    Guid ProviderId,
    string Name,
    string Email,
    string ServiceType,
    string Region,
    DateTime RegisteredAt
) : IntegrationEvent("ServiceProvider");