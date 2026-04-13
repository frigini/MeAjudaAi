using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar o evento de criação/aprovação de uma avaliação.
/// Atualiza a média de avaliação do provedor no módulo de busca.
/// </summary>
public sealed class ReviewCreatedIntegrationEventHandler(
    ISearchableProviderRepository repository,
    ILogger<ReviewCreatedIntegrationEventHandler> logger) : IEventHandler<ReviewCreatedIntegrationEvent>
{
    public async Task HandleAsync(ReviewCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Updating rating for provider {ProviderId} to {NewAverageRating} ({TotalReviews} reviews)",
                integrationEvent.ProviderId,
                integrationEvent.NewAverageRating,
                integrationEvent.TotalReviews);

            var provider = await repository.GetByProviderIdAsync(integrationEvent.ProviderId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found in search module when updating rating", integrationEvent.ProviderId);
                return;
            }

            provider.UpdateRating(integrationEvent.NewAverageRating, integrationEvent.TotalReviews);

            await repository.UpdateAsync(provider, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully updated rating for provider {ProviderId} in search module", integrationEvent.ProviderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ReviewCreatedIntegrationEvent for provider {ProviderId}", integrationEvent.ProviderId);
            // Considerar política de retry aqui se falhar
        }
    }
}
