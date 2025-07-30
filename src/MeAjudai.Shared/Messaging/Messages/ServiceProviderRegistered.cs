using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages;

public record ServiceProviderRegistered(
    Guid ProviderId,
    string Name,
    string Email,
    string ServiceType,
    string Region,
    DateTime RegisteredAt
) : IntegrationEvent("ServiceProvider");