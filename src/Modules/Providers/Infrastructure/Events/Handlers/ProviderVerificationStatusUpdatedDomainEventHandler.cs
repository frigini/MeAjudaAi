using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio ProviderVerificationStatusUpdatedDomainEvent e publica eventos de integração.
/// </summary>
internal sealed class ProviderVerificationStatusUpdatedDomainEventHandler(
    IMessageBus messageBus,
    ProvidersDbContext context,
    ILogger<ProviderVerificationStatusUpdatedDomainEventHandler> logger) : IEventHandler<ProviderVerificationStatusUpdatedDomainEvent>
{
    public async Task HandleAsync(ProviderVerificationStatusUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ProviderVerificationStatusUpdatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);

            var provider = await context.Providers
                .FirstOrDefaultAsync(p => p.Id == new Domain.ValueObjects.ProviderId(domainEvent.AggregateId), cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found when handling ProviderVerificationStatusUpdatedDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Publicar evento de integração para o Search module processar a indexação
            await messageBus.PublishAsync(new ProviderIndexRequiredIntegrationEvent(
                ModuleNames.Providers,
                provider.Id.Value), cancellationToken: cancellationToken);

            // Cria evento de integração para sistemas externos usando mapper
            var integrationEvent = domainEvent.ToIntegrationEvent(provider.UserId, provider.Name);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published ProviderVerificationStatusUpdated integration event for provider {ProviderId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProviderVerificationStatusUpdatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);
            throw;
        }
    }
}
