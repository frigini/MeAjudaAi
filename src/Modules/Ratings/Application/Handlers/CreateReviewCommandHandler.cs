using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Application.Handlers;

public sealed class CreateReviewCommandHandler(
    IReviewRepository repository,
    IContentModerator contentModerator,
    ILogger<CreateReviewCommandHandler> logger) : ICommandHandler<CreateReviewCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateReviewCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating review for provider {ProviderId} by customer {CustomerId}", command.ProviderId, command.CustomerId);

        // Validar duplicidade
        var existingReview = await repository.GetByProviderAndCustomerAsync(command.ProviderId, command.CustomerId, cancellationToken);
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

        // Moderação Automática
        var isClean = contentModerator.IsClean(command.Comment);

        if (!isClean)
        {
            logger.LogWarning("Review {ReviewId} flagged for moderation due to inappropriate content", review.Id.Value);
            review.MarkAsFlagged();
        }
        else if (command.Rating >= 4 && string.IsNullOrWhiteSpace(command.Comment))
        {
            // Auto-aprovação para notas altas sem comentário
            review.Approve();
        }

        await repository.AddAsync(review, cancellationToken);

        logger.LogInformation("Review {ReviewId} created with status {Status}", review.Id.Value, review.Status);

        return review.Id.Value;
    }
}
