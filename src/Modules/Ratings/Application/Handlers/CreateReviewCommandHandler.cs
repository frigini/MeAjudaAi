using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Modules.Ratings.Application.Handlers;

public sealed class CreateReviewCommandHandler(
    IReviewRepository repository,
    IContentModerator contentModerator,
    ILogger<CreateReviewCommandHandler> logger) : ICommandHandler<CreateReviewCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateReviewCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating review for provider {ProviderId} by customer {CustomerId}", command.ProviderId, command.CustomerId);

        // Validar duplicidade (UX check antecipado)
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
        // Short-circuit: Se não houver comentário, aplicar regra de auto-aprovação direta
        if (string.IsNullOrWhiteSpace(command.Comment))
        {
            if (command.Rating >= 4)
            {
                review.Approve();
            }
        }
        else
        {
            // Se houver comentário, passar pela moderação
            var isClean = contentModerator.IsClean(command.Comment);
            if (!isClean)
            {
                logger.LogWarning("Review {ReviewId} flagged for moderation due to inappropriate content", review.Id.Value);
                review.MarkAsFlagged();
            }
            // Se isClean for true, o status permanece Pending por padrão para moderação manual
        }

        try
        {
            await repository.AddAsync(review, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            logger.LogWarning(ex, "Unique constraint violation at DB level for Review between Provider {ProviderId} and Customer {CustomerId}", 
                command.ProviderId, command.CustomerId);
            throw new InvalidOperationException("Você já avaliou este prestador.");
        }

        logger.LogInformation("Review {ReviewId} created with status {Status}", review.Id.Value, review.Status);

        return review.Id.Value;
    }
}
