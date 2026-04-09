using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Consome o ProviderActivatedIntegrationEvent e enfileira e-mail de aprovação do prestador.
/// </summary>
internal sealed class ProviderActivatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogRepository logRepository,
    ILogger<ProviderActivatedIntegrationEventHandler> logger)
    : IEventHandler<ProviderActivatedIntegrationEvent>
{
    private const string TemplateKey = "provider_activated";

    public async Task HandleAsync(
        ProviderActivatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.ProviderId}";

        if (await logRepository.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping provider activation email for {ProviderId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, correlationId);
            return;
        }

        // NOTE: O e-mail do usuário virá do módulo Users via lookup futuro.
        // Por agora, loggamos com o ProviderId para rastreamento.
        var payload = JsonSerializer.Serialize(new
        {
            // Placeholder: em um cenário real, iriamos buscar o email via IUsersModuleApi
            To = $"provider_{integrationEvent.ProviderId}@placeholder.com",
            Subject = "Seu cadastro foi aprovado!",
            HtmlBody = $"<h1>Olá, {integrationEvent.Name}!</h1><p>Seu cadastro foi aprovado. Você já pode receber solicitações de serviço.</p>",
            TextBody = $"Olá, {integrationEvent.Name}!\nSeu cadastro foi aprovado. Você já pode receber solicitações de serviço.",
            From = (string?)null
        });

        var message = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: payload,
            priority: ECommunicationPriority.High);

        await outboxRepository.AddAsync(message, cancellationToken);

        logger.LogInformation(
            "Provider activation email enqueued for provider {ProviderId} (outboxId: {OutboxId}).",
            integrationEvent.ProviderId, message.Id);
    }
}
