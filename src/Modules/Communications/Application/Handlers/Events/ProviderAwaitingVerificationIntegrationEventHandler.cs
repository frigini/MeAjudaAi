using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Events;

/// <summary>
/// Handler para notificar administradores quando um prestador aguarda verificação.
/// </summary>
public sealed class ProviderAwaitingVerificationIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    IConfiguration configuration,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<ProviderAwaitingVerificationIntegrationEventHandler> logger)
    : IEventHandler<ProviderAwaitingVerificationIntegrationEvent>
{
    public async Task HandleAsync(ProviderAwaitingVerificationIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var adminEmail = configuration[CommunicationConstants.AdminEmailConfigKey];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            adminEmail = CommunicationConstants.DefaultAdminEmail;
        }
        
        var correlationId = $"{CommunicationTemplateKeys.ProviderAwaitingVerification}{CommunicationConstants.CorrelationSeparator}{integrationEvent.ProviderId}";

        var emailPayload = new
        {
            To = adminEmail,
            Subject = "Novo prestador aguardando verificação",
            Body = $"O prestador {integrationEvent.Name} (ID: {integrationEvent.ProviderId}) enviou documentos para análise.",
            TemplateKey = CommunicationTemplateKeys.ProviderAwaitingVerification,
            CorrelationId = correlationId
        };

        var message = OutboxMessage.Create(
            ECommunicationChannel.Email,
            serializer.Serialize(emailPayload),
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
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

            if ((processedException is Shared.Database.Exceptions.UniqueConstraintException))
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
