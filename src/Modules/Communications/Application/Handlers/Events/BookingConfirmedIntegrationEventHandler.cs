using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Users;
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

public sealed class BookingConfirmedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IProvidersModuleApi providersApi,
    IUsersModuleApi usersApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<BookingConfirmedIntegrationEventHandler> logger)
    : IEventHandler<BookingConfirmedIntegrationEvent>
{
    private const string TemplateKey = "booking_confirmed";

    public async Task HandleAsync(
        BookingConfirmedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.BookingId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping booking confirmed notification for {BookingId} — already sent.",
                integrationEvent.BookingId);
            return;
        }

        var providerResult = await providersApi.GetProviderByIdAsync(integrationEvent.ProviderId, cancellationToken);
        var clientResult = await usersApi.GetUserByIdAsync(integrationEvent.ClientId, cancellationToken);

        if (providerResult.IsFailure || providerResult.Value == null || clientResult.IsFailure || clientResult.Value == null)
        {
            logger.LogWarning("Could not find provider or client for notification.");
            return;
        }

        var providerName = providerResult.Value.Name;
        var clientEmail = clientResult.Value.Email;

        // E-mail payload
        var clientPayload = serializer.Serialize(new
        {
            To = clientEmail,
            Subject = "Agendamento Confirmado!",
            HtmlBody = $"<h1>Tudo certo!</h1><p>Seu agendamento com {providerName} foi confirmado.</p>",
            TextBody = $"Tudo certo! Seu agendamento com {providerName} foi confirmado.",
            CorrelationId = correlationId,
            TemplateKey = TemplateKey
        });

        var clientMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: clientPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: correlationId);

        // Push payload
        var pushMessage = clientResult.Value.DeviceToken != null 
            ? OutboxMessage.Create(
                channel: ECommunicationChannel.Push,
                payload: serializer.Serialize(new { 
                    DeviceToken = clientResult.Value.DeviceToken, 
                    Title = "Agendamento Confirmado!", 
                    Body = $"Seu agendamento com {providerName} foi confirmado." 
                }),
                maxRetries: 3,
                priority: ECommunicationPriority.Normal,
                correlationId: $"{correlationId}:push")
            : null;

        // SMS para o cliente
        var clientSms = clientResult.Value.PhoneNumber != null
            ? OutboxMessage.Create(
                channel: ECommunicationChannel.Sms,
                payload: serializer.Serialize(new { 
                    PhoneNumber = clientResult.Value.PhoneNumber, 
                    Body = $"Seu agendamento com {providerName} foi confirmado." 
                }),
                maxRetries: 3,
                priority: ECommunicationPriority.Normal,
                correlationId: $"{correlationId}:client:sms")
            : null;

        try
        {
            await outboxRepository.AddAsync(clientMessage, cancellationToken);
            if (pushMessage != null) await outboxRepository.AddAsync(pushMessage, cancellationToken);
            if (clientSms != null) await outboxRepository.AddAsync(clientSms, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Booking confirmed notification (email, push, and sms) enqueued for {BookingId}.", integrationEvent.BookingId);
        }
        catch (Exception ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(
                ex as DbUpdateException ?? new DbUpdateException(ex.Message, ex));

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping booking confirmed notification for {BookingId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.BookingId, correlationId);
                return;
            }

            logger.LogError(ex, "Error enqueuing booking confirmed notification for {BookingId}.", integrationEvent.BookingId);
            throw;
        }
    }
}
