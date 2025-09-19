using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.ServiceProvider;

public record ServiceProviderRegisteredIntegrationEvent(
    Guid ProviderId,
    string Name,
    string Email,
    string ServiceType,
    string Region,
    DateTime RegisteredAt
) : IntegrationEvent("ServiceProvider");