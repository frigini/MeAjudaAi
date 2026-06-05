using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.Application.Handlers;

public sealed class CreateReviewCommandHandler(
    [FromKeyedServices(ModuleKeys.Ratings)] IUnitOfWork uow,
    IReviewQueries queries,
    IContentModerator contentModerator,
    ILogger<CreateReviewCommandHandler> logger) : ICommandHandler<CreateReviewCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateReviewCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating review for provider {ProviderId} by customer {CustomerId}", command.ProviderId, command.CustomerId);

        var existingReview = await queries.GetByProviderAndCustomerAsync(command.ProviderId, command.CustomerId, cancellationToken);
        if (existingReview != null)
        {
            logger.LogWarning("Customer {CustomerId} already reviewed provider {ProviderId}", command.CustomerId, command.ProviderId);
            throw new InvalidOperationException("Você já avaliou este prestador.");
        }

        var review = Review.Create(
            command.ProviderId,
            command.CustomerId,
            command.Rating,
            command.Comment);

        if (string.IsNullOrWhiteSpace(command.Comment))
        {
            if (command.Rating >= 4)
            {
                review.Approve();
            }
        }
        else
        {
            var isClean = contentModerator.IsClean(command.Comment);
            if (!isClean)
            {
                logger.LogWarning("Review {ReviewId} flagged for moderation due to inappropriate content", review.Id.Value);
                review.MarkAsFlagged();
            }
        }

        try
        {
            uow.GetRepository<Review, ReviewId>().Add(review);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            logger.LogWarning(ex, "Duplicate review detected at persistence level for Provider {ProviderId} and Customer {CustomerId}",
                command.ProviderId, command.CustomerId);
            throw new InvalidOperationException("Você já avaliou este prestador.");
        }

        logger.LogInformation("Review {ReviewId} created with status {Status}", review.Id.Value, review.Status);

        return review.Id.Value;
    }
}


