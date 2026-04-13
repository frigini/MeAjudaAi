using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;

public sealed class ReviewRejectedDomainEventHandler(
    ILogger<ReviewRejectedDomainEventHandler> logger) : IEventHandler<ReviewRejectedDomainEvent>
{
    public Task HandleAsync(ReviewRejectedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Review {ReviewId} for provider {ProviderId} was rejected. Reason: {Reason}", 
            domainEvent.AggregateId, domainEvent.ProviderId, domainEvent.Reason);
            
        return Task.CompletedTask;
    }
}
