using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Handler para notificar administradores quando um prestador aguarda verificação.
/// </summary>
internal sealed class ProviderAwaitingVerificationIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ILogger<ProviderAwaitingVerificationIntegrationEventHandler> logger)
    : IEventHandler<ProviderAwaitingVerificationIntegrationEvent>
{
    public async Task HandleAsync(ProviderAwaitingVerificationIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        // No MVP, apenas simulamos o envio de um e-mail para o suporte/admin
        var emailPayload = new
        {
            To = "admin@meajudaai.com.br",
            Subject = "Novo prestador aguardando verificação",
            Body = $"O prestador {integrationEvent.Name} (ID: {integrationEvent.ProviderId}) enviou documentos para análise.",
            TemplateKey = "admin-provider-verification-alert"
        };

        var message = OutboxMessage.Create(
            ECommunicationChannel.Email,
            JsonSerializer.Serialize(emailPayload),
            ECommunicationPriority.Normal);

        await outboxRepository.AddAsync(message, cancellationToken);
        await outboxRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Admin notification enqueued for provider {ProviderId}.", integrationEvent.ProviderId);
    }
}
