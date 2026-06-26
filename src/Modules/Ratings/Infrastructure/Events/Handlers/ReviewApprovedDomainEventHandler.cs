using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;

internal sealed class ReviewApprovedDomainEventHandler(
    IMessageBus messageBus,
    IReviewQueries queries,
    ICacheService cacheService,
    ILogger<ReviewApprovedDomainEventHandler> logger) : IEventHandler<ReviewApprovedDomainEvent>
{
    public async Task HandleAsync(ReviewApprovedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ReviewApprovedDomainEvent for provider {ProviderId}", domainEvent.ProviderId);

            var (average, total) = await queries.GetAverageRatingForProviderAsync(domainEvent.ProviderId, cancellationToken);

            await cacheService.RemoveByTagAsync(CacheTags.ReviewTag(domainEvent.AggregateId), cancellationToken);

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
