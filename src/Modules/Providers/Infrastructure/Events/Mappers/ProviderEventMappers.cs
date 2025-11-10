using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;

/// <summary>
/// Mappers para converter Domain Events em Integration Events do módulo Providers.
/// </summary>
public static class ProviderEventMappers
{
    private const string ModuleName = "Providers";

    /// <summary>
    /// Converte ProviderRegisteredDomainEvent para ProviderRegisteredIntegrationEvent.
    /// </summary>
    public static ProviderRegisteredIntegrationEvent ToIntegrationEvent(this ProviderRegisteredDomainEvent domainEvent)
    {
        return new ProviderRegisteredIntegrationEvent(
            Source: ModuleName,
            ProviderId: domainEvent.AggregateId,
            UserId: domainEvent.UserId,
            Name: domainEvent.Name,
            ProviderType: domainEvent.Type.ToString(),
            Email: domainEvent.Email,
            RegisteredAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Converte ProviderDeletedDomainEvent para ProviderDeletedIntegrationEvent.
    /// </summary>
    public static ProviderDeletedIntegrationEvent ToIntegrationEvent(this ProviderDeletedDomainEvent domainEvent, Guid userId)
    {
        return new ProviderDeletedIntegrationEvent(
            Source: ModuleName,
            ProviderId: domainEvent.AggregateId,
            UserId: userId,
            Name: domainEvent.Name,
            Reason: "Provider deleted",
            DeletedAt: DateTime.UtcNow,
            DeletedBy: domainEvent.DeletedBy
        );
    }

    /// <summary>
    /// Converte ProviderVerificationStatusUpdatedDomainEvent para ProviderVerificationStatusUpdatedIntegrationEvent.
    /// </summary>
    public static ProviderVerificationStatusUpdatedIntegrationEvent ToIntegrationEvent(this ProviderVerificationStatusUpdatedDomainEvent domainEvent, Guid userId, string providerName)
    {
        return new ProviderVerificationStatusUpdatedIntegrationEvent(
            Source: ModuleName,
            ProviderId: domainEvent.AggregateId,
            UserId: userId,
            Name: providerName,
            PreviousStatus: domainEvent.PreviousStatus.ToString(),
            NewStatus: domainEvent.NewStatus.ToString(),
            UpdatedBy: domainEvent.UpdatedBy
        );
    }

    /// <summary>
    /// Converte ProviderProfileUpdatedDomainEvent para ProviderProfileUpdatedIntegrationEvent.
    /// </summary>
    public static ProviderProfileUpdatedIntegrationEvent ToIntegrationEvent(this ProviderProfileUpdatedDomainEvent domainEvent, Guid userId, string[] updatedFields)
    {
        return new ProviderProfileUpdatedIntegrationEvent(
            Source: ModuleName,
            ProviderId: domainEvent.AggregateId,
            UserId: userId,
            Name: domainEvent.Name,
            UpdatedFields: updatedFields,
            UpdatedBy: domainEvent.UpdatedBy,
            NewEmail: domainEvent.Email
        );
    }
}