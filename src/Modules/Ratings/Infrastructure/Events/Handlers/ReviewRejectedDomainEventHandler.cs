using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;

public sealed class ReviewRejectedDomainEventHandler(
    ILogger<ReviewRejectedDomainEventHandler> logger) : IEventHandler<ReviewRejectedDomainEvent>
{
    public Task HandleAsync(ReviewRejectedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Review {ReviewId} for provider {ProviderId} was rejected.", 
            domainEvent.AggregateId, domainEvent.ProviderId);
            
        var reasonPreview = domainEvent.Reason?.Length > 100 
            ? domainEvent.Reason[..100] + "..." 
            : domainEvent.Reason;
            
        logger.LogDebug("Rejection reason for Review {ReviewId}: {Reason}", 
            domainEvent.AggregateId, reasonPreview);
            
        return Task.CompletedTask;
    }
}
