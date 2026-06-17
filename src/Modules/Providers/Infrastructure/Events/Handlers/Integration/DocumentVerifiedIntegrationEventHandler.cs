using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para processar eventos de integração de documentos verificados.
/// </summary>
internal sealed class DocumentVerifiedIntegrationEventHandler(
    IProviderQueries providerQueries,
    ILogger<DocumentVerifiedIntegrationEventHandler> logger) : IEventHandler<DocumentVerifiedIntegrationEvent>
{
    public async Task HandleAsync(DocumentVerifiedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling DocumentVerifiedIntegrationEvent for provider {ProviderId}, document {DocumentId}",
                integrationEvent.ProviderId,
                integrationEvent.DocumentId);

            var (exists, status) = await providerQueries.GetProviderStatusAsync(
                new ProviderId(integrationEvent.ProviderId),
                cancellationToken);

            if (!exists)
            {
                logger.LogWarning(
                    "Provider {ProviderId} not found when handling DocumentVerifiedIntegrationEvent for document {DocumentId}",
                    integrationEvent.ProviderId,
                    integrationEvent.DocumentId);
                return;
            }

            logger.LogInformation(
                "Document {DocumentId} of type {DocumentType} verified for provider {ProviderId}. Provider status: {Status}",
                integrationEvent.DocumentId,
                integrationEvent.DocumentType,
                integrationEvent.ProviderId,
                status);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling DocumentVerifiedIntegrationEvent for provider {ProviderId}, document {DocumentId}",
                integrationEvent.ProviderId,
                integrationEvent.DocumentId);
            throw new InvalidOperationException(
                $"Failed to process DocumentVerified integration event for provider '{integrationEvent.ProviderId}', document '{integrationEvent.DocumentId}'",
                ex);
        }
    }
}





