using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar o evento de aprovação de uma avaliação.
/// Atualiza a média de avaliação do provedor no módulo de busca.
/// </summary>
internal sealed class ReviewApprovedIntegrationEventHandler(
    [FromKeyedServices(ModuleKeys.SearchProviders)] IUnitOfWork uow,
    ISearchableProviderQueries queries,
    ILogger<ReviewApprovedIntegrationEventHandler> logger) : IEventHandler<ReviewApprovedIntegrationEvent>
{
    public async Task HandleAsync(ReviewApprovedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Updating rating for provider {ProviderId} to {NewAverageRating} ({TotalReviews} reviews)",
                integrationEvent.ProviderId,
                integrationEvent.NewAverageRating,
                integrationEvent.TotalReviews);

            var provider = await queries.GetByProviderIdAsync(integrationEvent.ProviderId, track: true, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found in search module when updating rating", integrationEvent.ProviderId);
                return;
            }

            provider.UpdateRating(integrationEvent.NewAverageRating, integrationEvent.TotalReviews);

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully updated rating for provider {ProviderId} in search module", integrationEvent.ProviderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ReviewApprovedIntegrationEvent for provider {ProviderId}", integrationEvent.ProviderId);
            throw; // Rethrow para permitir retry pelo broker
        }
    }
}



