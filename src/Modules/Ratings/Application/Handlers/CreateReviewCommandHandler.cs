using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Modules.Ratings.Application.Handlers;

public sealed class CreateReviewCommandHandler(
    IUnitOfWork uow,
    IReviewQueries queries,
    IContentModerator contentModerator,
    ILogger<CreateReviewCommandHandler> logger) : ICommandHandler<CreateReviewCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateReviewCommand command, CancellationToken cancellationToken = default)
    {
        var diagPath = @"C:\Code\MeAjudaAi\tests\MeAjudaAi.E2E.Tests\bin\Debug\net10.0\db_diag.log";
        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] HandleAsync starting for provider {command.ProviderId}...{System.Environment.NewLine}");
        logger.LogInformation("Creating review for provider {ProviderId} by customer {CustomerId}", command.ProviderId, command.CustomerId);

        // Validar duplicidade (UX check antecipado)
        var existingReview = await queries.GetByProviderAndCustomerAsync(command.ProviderId, command.CustomerId, cancellationToken);
        if (existingReview != null)
        {
            logger.LogWarning("Customer {CustomerId} already reviewed provider {ProviderId}", command.CustomerId, command.ProviderId);
            throw new InvalidOperationException("Você já avaliou este prestador.");
        }

        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] Creating review object...{System.Environment.NewLine}");
        var review = Review.Create(
            command.ProviderId,
            command.CustomerId,
            command.Rating,
            command.Comment);
        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] Review object created. ID: {review.Id.Value}{System.Environment.NewLine}");

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
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] Calling contentModerator.IsClean for review {review.Id.Value}...{System.Environment.NewLine}");
            var isClean = contentModerator.IsClean(command.Comment);
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] contentModerator.IsClean completed. IsClean: {isClean}{System.Environment.NewLine}");
            
            if (!isClean)
            {
                logger.LogWarning("Review {ReviewId} flagged for moderation due to inappropriate content", review.Id.Value);
                review.MarkAsFlagged();
            }
            // Se isClean for true, o status permanece Pending por padrão para moderação manual
        }

        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] Logic blocks completed. Adding to repository...{System.Environment.NewLine}");
        try
        {
            uow.GetRepository<Review, ReviewId>().Add(review);
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] Added to repository. Calling uow.SaveChangesAsync...{System.Environment.NewLine}");
            await uow.SaveChangesAsync(cancellationToken);
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] uow.SaveChangesAsync completed.{System.Environment.NewLine}");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] Duplicate review detected.{System.Environment.NewLine}");
            logger.LogWarning(ex, "Duplicate review detected at persistence level for Provider {ProviderId} and Customer {CustomerId}", 
                command.ProviderId, command.CustomerId);
            throw new InvalidOperationException("Você já avaliou este prestador.");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [HANDLER] Error: {ex.Message}{System.Environment.NewLine}");
            throw;
        }

        logger.LogInformation("Review {ReviewId} created with status {Status}", review.Id.Value, review.Status);

        return review.Id.Value;
    }
}
