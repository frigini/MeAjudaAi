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
            
        if (!string.IsNullOrWhiteSpace(domainEvent.Reason))
        {
            var reason = domainEvent.Reason;
            var reasonPreview = reason.Length > 100 
                ? new string(new System.Globalization.StringInfo(reason).SubstringByTextElements(0, 100).ToCharArray()) + "..." 
                : reason;

            logger.LogDebug("Rejection reason for Review {ReviewId}: {Reason}", 
                domainEvent.AggregateId, reasonPreview);
        }
            
        return Task.CompletedTask;
    }
}
