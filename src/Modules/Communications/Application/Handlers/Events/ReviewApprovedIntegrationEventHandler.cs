using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Events;

public sealed class ReviewApprovedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IProvidersModuleApi providersModuleApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<ReviewApprovedIntegrationEventHandler> logger)
    : IEventHandler<ReviewApprovedIntegrationEvent>
{
    private const string TemplateKey = "review_approved";

    public async Task HandleAsync(
        ReviewApprovedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.ReviewId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping review approved email for provider {ProviderId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, correlationId);
            return;
        }

        var providerResult = await providersModuleApi.GetProviderByIdAsync(integrationEvent.ProviderId, cancellationToken);
        if (!providerResult.IsSuccess)
        {
            if (providerResult.Error?.StatusCode == StatusCodes.Status404NotFound || providerResult.Value == null)
            {
                logger.LogWarning("Provider {ProviderId} not found for review {ReviewId}.", integrationEvent.ProviderId, integrationEvent.ReviewId);
                return;
            }
            
            logger.LogError("Failed to get provider {ProviderId} for review {ReviewId}: {Error}", 
                integrationEvent.ProviderId, integrationEvent.ReviewId, providerResult.Error?.Message);
            throw new Exception("Transient provider lookup failure");
        }
        
        var provider = providerResult.Value!;
        var payload = serializer.Serialize(new
        {
            To = provider.Email,
            Subject = "Nova Avaliação Aprovada!",
            HtmlBody = $"<h1>Olá, {provider.Name}!</h1><p>Uma nova avaliação foi aprovada.</p>",
            TextBody = $"Olá, {provider.Name}!\nUma nova avaliação foi aprovada.",
            TemplateKey = TemplateKey
        });

        var message = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: payload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Review approved email enqueued for provider {ProviderId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, message.Id, correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue review approved email for {ProviderId}.", integrationEvent.ProviderId);
            throw;
        }
    }
}
