using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;

public sealed class ReviewApprovedDomainEventHandler(
    IMessageBus messageBus,
    IReviewQueries queries,
    ILogger<ReviewApprovedDomainEventHandler> logger) : IEventHandler<ReviewApprovedDomainEvent>
{
    public async Task HandleAsync(ReviewApprovedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var diagPath = @"C:\Code\MeAjudaAi\tests\MeAjudaAi.E2E.Tests\bin\Debug\net10.0\db_diag.log";
        try
        {
            await System.IO.File.AppendAllTextAsync(diagPath, $"[{System.DateTime.UtcNow:O}] [EVENT] ReviewApprovedDomainEventHandler starting for provider {domainEvent.ProviderId}...{System.Environment.NewLine}");
            logger.LogInformation("Handling ReviewApprovedDomainEvent for provider {ProviderId}", domainEvent.ProviderId);

            // Calcula a nova média
            await System.IO.File.AppendAllTextAsync(diagPath, $"[{System.DateTime.UtcNow:O}] [EVENT] Calling queries.GetAverageRatingForProviderAsync...{System.Environment.NewLine}");
            var (average, total) = await queries.GetAverageRatingForProviderAsync(domainEvent.ProviderId, cancellationToken);
            await System.IO.File.AppendAllTextAsync(diagPath, $"[{System.DateTime.UtcNow:O}] [EVENT] queries.GetAverageRatingForProviderAsync completed. Average: {average}, Total: {total}{System.Environment.NewLine}");

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
