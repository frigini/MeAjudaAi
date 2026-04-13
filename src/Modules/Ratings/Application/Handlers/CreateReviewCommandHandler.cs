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

        // Validar duplicidade (UX check)
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

        // Moderação Automática e Regras de Auto-aprovação
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
            await repository.AddAsync(review, cancellationToken);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Concurrency/Duplicate detected at DB level for Review between Provider {ProviderId} and Customer {CustomerId}", 
                command.ProviderId, command.CustomerId);
            throw new InvalidOperationException("Você já avaliou este prestador.");
        }

        logger.LogInformation("Review {ReviewId} created with status {Status}", review.Id.Value, review.Status);

        return review.Id.Value;
    }

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        // Detecta violação de unicidade (Postgres 23505)
        var message = ex.ToString();
        return message.Contains("23505") || message.Contains("unique constraint") || (ex.InnerException?.Message.Contains("23505") ?? false);
    }
}
