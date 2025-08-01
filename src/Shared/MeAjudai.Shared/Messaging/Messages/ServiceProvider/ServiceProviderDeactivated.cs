using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.ServiceProvider;

public record ServiceProviderDeactivated(
    Guid ProviderId,
    string Reason,
    DateTime DeactivatedAt
) : IntegrationEvent("ServiceProvider");