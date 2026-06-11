using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Events;

public sealed class ProviderDeletedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<ProviderDeletedIntegrationEventHandler> logger)
    : IEventHandler<ProviderDeletedIntegrationEvent>
{
    private const string TemplateKey = "provider_deleted";

    public async Task HandleAsync(
        ProviderDeletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.ProviderId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping provider deleted email for provider {ProviderId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, correlationId);
            return;
        }

        var email = integrationEvent.Email;
        var name = integrationEvent.Name;

        var payload = serializer.Serialize(new
        {
            To = email,
            Subject = "Conta de Prestador Excluída",
            HtmlBody = $"<h1>Olá, {name}!</h1><p>Sua conta de prestador no MeAjudaAi foi excluída com sucesso.</p>",
            TextBody = $"Olá, {name}!\nSua conta de prestador no MeAjudaAi foi excluída com sucesso.",
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
                "Provider deleted email enqueued for provider {ProviderId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, message.Id, correlationId);
        }
        catch (Exception ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(
                ex as DbUpdateException ?? new DbUpdateException(ex.Message, ex));

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping provider deleted email for {ProviderId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.ProviderId, correlationId);
                return;
            }

            logger.LogError(ex, "Failed to enqueue provider deleted email for {ProviderId}.", integrationEvent.ProviderId);
            throw;
        }
    }
}
