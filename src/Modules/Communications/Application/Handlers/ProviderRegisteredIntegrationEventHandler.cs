using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

public sealed class ProviderRegisteredIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    ILogger<ProviderRegisteredIntegrationEventHandler> logger)
    : IEventHandler<ProviderRegisteredIntegrationEvent>
{
    private const string TemplateKey = "provider_registered";

    public async Task HandleAsync(
        ProviderRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.ProviderId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            To = integrationEvent.Email,
            Subject = "Bem-vindo ao MeAjudaAi - Cadastro de Prestador",
            HtmlBody = $"<h1>Olá, {integrationEvent.Name}!</h1><p>Seu cadastro como prestador foi recebido e está em análise.</p>",
            TextBody = $"Olá, {integrationEvent.Name}! Seu cadastro como prestador foi recebido e está em análise.",
            CorrelationId = correlationId,
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

            logger.LogInformation("Provider welcome email enqueued for {ProviderId}.", integrationEvent.ProviderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enqueuing provider welcome email for {ProviderId}.", integrationEvent.ProviderId);
        }
    }
}
