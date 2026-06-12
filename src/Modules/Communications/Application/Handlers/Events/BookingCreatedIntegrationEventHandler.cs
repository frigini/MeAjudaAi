using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

public sealed class BookingCreatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IProvidersModuleApi providersApi,
    IUsersModuleApi usersApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<BookingCreatedIntegrationEventHandler> logger)
    : IEventHandler<BookingCreatedIntegrationEvent>
{
    private const string TemplateKey = CommunicationTemplateKeys.BookingCreated;

    public async Task HandleAsync(
        BookingCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.BookingId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping booking created notification for {BookingId} — already sent.",
                integrationEvent.BookingId);
            return;
        }

        // Buscar dados do provedor para obter o UserId e Email
        var providerResult = await providersApi.GetProviderByIdAsync(integrationEvent.ProviderId, cancellationToken);
        var clientResult = await usersApi.GetUserByIdAsync(integrationEvent.ClientId, cancellationToken);

        if (providerResult.IsFailure || providerResult.Value == null)
        {
            logger.LogWarning("Could not find provider {ProviderId} for notification.", integrationEvent.ProviderId);
            return;
        }

        if (clientResult.IsFailure || clientResult.Value == null)
        {
            logger.LogWarning("Could not find client {ClientId} for notification.", integrationEvent.ClientId);
            return;
        }

        var providerEmail = providerResult.Value.Email; // ModuleProviderDto possui Email
        var clientEmail = clientResult.Value.Email;
        var clientName = clientResult.Value.FirstName;

        // Notificar o Provedor
        var providerPayload = serializer.Serialize(new
        {
            To = providerEmail,
            Subject = "Novo Agendamento Recebido",
            HtmlBody = $"<h1>Novo Agendamento!</h1><p>Você recebeu um novo agendamento de {clientName} para o dia {integrationEvent.Date}.</p>",
            TextBody = $"Novo Agendamento! Você recebeu um novo agendamento de {clientName} para o dia {integrationEvent.Date}.",
            CorrelationId = $"{correlationId}:provider"
        });

        var providerMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: providerPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.High,
            correlationId: $"{correlationId}:provider");

        // Push para o provedor
        var providerPush = providerResult.Value.DeviceToken != null
            ? OutboxMessage.Create(
                channel: ECommunicationChannel.Push,
                payload: serializer.Serialize(new { 
                    DeviceToken = providerResult.Value.DeviceToken, 
                    Title = "Novo Agendamento!", 
                    Body = $"Novo agendamento de {clientName} para o dia {integrationEvent.Date}." 
                }),
                maxRetries: 3,
                priority: ECommunicationPriority.High,
                correlationId: $"{correlationId}:provider:push")
            : null;

        // Notificar o Cliente
        var clientPayload = serializer.Serialize(new
        {
            To = clientEmail,
            Subject = "Agendamento Solicitado",
            HtmlBody = $"<h1>Solicitação Enviada!</h1><p>Seu agendamento para o dia {integrationEvent.Date} foi enviado e aguarda confirmação do prestador.</p>",
            TextBody = $"Solicitação Enviada! Seu agendamento para o dia {integrationEvent.Date} foi enviado e aguarda confirmação do prestador.",
            CorrelationId = $"{correlationId}:client"
        });

        var clientMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: clientPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: $"{correlationId}:client");

        // SMS para o cliente
        var clientSms = clientResult.Value.PhoneNumber != null
            ? OutboxMessage.Create(
                channel: ECommunicationChannel.Sms,
                payload: serializer.Serialize(new { 
                    PhoneNumber = clientResult.Value.PhoneNumber, 
                    Body = $"Agendamento solicitado com sucesso para o dia {integrationEvent.Date}." 
                }),
                maxRetries: 3,
                priority: ECommunicationPriority.Normal,
                correlationId: $"{correlationId}:client:sms")
            : null;

        try
        {
            await outboxRepository.AddAsync(providerMessage, cancellationToken);
            if (providerPush != null) await outboxRepository.AddAsync(providerPush, cancellationToken);
            await outboxRepository.AddAsync(clientMessage, cancellationToken);
            if (clientSms != null) await outboxRepository.AddAsync(clientSms, cancellationToken);
            
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Booking created notifications enqueued for {BookingId}.",
                integrationEvent.BookingId);
        }
        catch (Exception ex)
        {
            if (ex is not DbUpdateException dbEx)
            {
                logger.LogError(ex, "Error enqueuing booking created notifications for {BookingId}.", integrationEvent.BookingId);
                throw;
            }

            var processedException = PostgreSqlExceptionProcessor.ProcessException(dbEx);

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping booking created notifications for {BookingId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.BookingId, correlationId);
                return;
            }

            logger.LogError(ex, "Error enqueuing booking created notifications for {BookingId}.", integrationEvent.BookingId);
            throw;
        }
    }
}
