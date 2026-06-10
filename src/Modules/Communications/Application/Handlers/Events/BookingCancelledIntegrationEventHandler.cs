using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

public sealed class BookingCancelledIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IProvidersModuleApi providersApi,
    IUsersModuleApi usersApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<BookingCancelledIntegrationEventHandler> logger)
    : IEventHandler<BookingCancelledIntegrationEvent>
{
    private const string TemplateKey = "booking_cancelled";

    public async Task HandleAsync(
        BookingCancelledIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.BookingId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping booking cancelled notification for {BookingId} — already sent.",
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

        var providerEmail = providerResult.Value.Email;
        var clientEmail = clientResult.Value.Email;

        // Notificar ambos sobre o cancelamento
        var reason = string.IsNullOrWhiteSpace(integrationEvent.Reason) ? "Não informada" : integrationEvent.Reason;

        var providerPayload = serializer.Serialize(new
        {
            To = providerEmail,
            Subject = "Agendamento Cancelado",
            HtmlBody = $"<h1>Agendamento Cancelado</h1><p>O agendamento {integrationEvent.BookingId} foi cancelado.</p><p>Motivo: {reason}</p>",
            TextBody = $"Agendamento Cancelado. O agendamento {integrationEvent.BookingId} foi cancelado. Motivo: {reason}",
            TemplateKey = TemplateKey
        });

        var clientPayload = serializer.Serialize(new
        {
            To = clientEmail,
            Subject = "Agendamento Cancelado",
            HtmlBody = $"<h1>Agendamento Cancelado</h1><p>O agendamento {integrationEvent.BookingId} foi cancelado.</p><p>Motivo: {reason}</p>",
            TextBody = $"Agendamento Cancelado. O agendamento {integrationEvent.BookingId} foi cancelado. Motivo: {reason}",
            TemplateKey = TemplateKey
        });

        var providerMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: providerPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: $"{correlationId}:provider");

        var clientMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: clientPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: $"{correlationId}:client");

        // SMS para o cliente
        var clientSms = !string.IsNullOrWhiteSpace(clientResult.Value.PhoneNumber)
            ? OutboxMessage.Create(
                channel: ECommunicationChannel.Sms,
                payload: serializer.Serialize(new { 
                    PhoneNumber = clientResult.Value.PhoneNumber, 
                    Body = $"Seu agendamento {integrationEvent.BookingId} foi cancelado. Motivo: {reason}" 
                }),
                maxRetries: 3,
                priority: ECommunicationPriority.Normal,
                correlationId: $"{correlationId}:client:sms")
            : null;

        try
        {
            await outboxRepository.AddAsync(providerMessage, cancellationToken);
            await outboxRepository.AddAsync(clientMessage, cancellationToken);
            if (clientSms != null) await outboxRepository.AddAsync(clientSms, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Booking cancelled notifications enqueued for {BookingId}.", integrationEvent.BookingId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enqueuing booking cancelled notifications for {BookingId}.", integrationEvent.BookingId);
            throw;
        }
    }
}
