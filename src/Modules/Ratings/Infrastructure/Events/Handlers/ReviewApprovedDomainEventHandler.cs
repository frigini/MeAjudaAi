using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;

public sealed class ReviewApprovedDomainEventHandler(
    IMessageBus messageBus,
    IReviewRepository repository,
    ILogger<ReviewApprovedDomainEventHandler> logger) : IEventHandler<ReviewApprovedDomainEvent>
{
    public async Task HandleAsync(ReviewApprovedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ReviewApprovedDomainEvent for provider {ProviderId}", domainEvent.ProviderId);

            // Calcula a nova média
            var (average, total) = await repository.GetAverageRatingForProviderAsync(domainEvent.ProviderId, cancellationToken);

            var integrationEvent = new ReviewApprovedIntegrationEvent(
                Source: "Ratings",
                ProviderId: domainEvent.ProviderId,
                ReviewId: domainEvent.AggregateId,
                NewAverageRating: average,
                TotalReviews: total,
                ReviewRating: domainEvent.Rating,
                ReviewComment: domainEvent.Comment,
                CreatedAt: DateTime.UtcNow
            );

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published ReviewApprovedIntegrationEvent for provider {ProviderId} (Average: {Average}, Total: {Total})", 
                domainEvent.ProviderId, average, total);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ReviewApprovedDomainEvent for provider {ProviderId}", domainEvent.ProviderId);
            throw;
        }
    }
}
