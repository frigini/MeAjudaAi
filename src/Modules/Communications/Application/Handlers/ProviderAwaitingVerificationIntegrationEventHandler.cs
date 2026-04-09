using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Contracts.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Handler para notificar administradores quando um prestador aguarda verificação.
/// </summary>
internal sealed class ProviderAwaitingVerificationIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    IConfiguration configuration,
    ILogger<ProviderAwaitingVerificationIntegrationEventHandler> logger)
    : IEventHandler<ProviderAwaitingVerificationIntegrationEvent>
{
    public async Task HandleAsync(ProviderAwaitingVerificationIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var adminEmail = configuration["Communications:AdminEmail"] ?? "suporte@meajudaai.com.br";
        var correlationId = $"admin_verification_alert:{integrationEvent.ProviderId}";

        var emailPayload = new
        {
            To = adminEmail,
            Subject = "Novo prestador aguardando verificação",
            Body = $"O prestador {integrationEvent.Name} (ID: {integrationEvent.ProviderId}) enviou documentos para análise.",
            TemplateKey = "admin-provider-verification-alert",
            CorrelationId = correlationId
        };

        var message = OutboxMessage.Create(
            ECommunicationChannel.Email,
            JsonSerializer.Serialize(emailPayload),
            ECommunicationPriority.Normal,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Admin notification enqueued for provider {ProviderId} (correlationId: {CorrelationId}).", 
                integrationEvent.ProviderId, correlationId);
        }
        catch (Exception ex)
        {
            var processedException = MeAjudaAi.Shared.Database.Exceptions.PostgreSqlExceptionProcessor.ProcessException(
                ex as Microsoft.EntityFrameworkCore.DbUpdateException ?? new Microsoft.EntityFrameworkCore.DbUpdateException(ex.Message, ex));

            if (processedException is MeAjudaAi.Shared.Database.Exceptions.UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping admin notification for provider {ProviderId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.ProviderId, correlationId);
                return;
            }
            
            throw;
        }
    }
}
