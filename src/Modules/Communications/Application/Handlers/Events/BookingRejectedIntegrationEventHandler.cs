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

public sealed class BookingRejectedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IProvidersModuleApi providersApi,
    IUsersModuleApi usersApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<BookingRejectedIntegrationEventHandler> logger)
    : IEventHandler<BookingRejectedIntegrationEvent>
{
    private const string TemplateKey = CommunicationTemplateKeys.BookingRejected;

    public async Task HandleAsync(
        BookingRejectedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.BookingId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping booking rejected notification for {BookingId} — already sent.",
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

        var reason = string.IsNullOrWhiteSpace(integrationEvent.Reason) ? "Não informado" : integrationEvent.Reason;

        var clientPayload = serializer.Serialize(new
        {
            To = clientEmail,
            Subject = "Agendamento Rejeitado",
            HtmlBody = $"<h1>Agendamento Rejeitado</h1><p>Seu agendamento com {providerName} foi rejeitado.</p><p>Motivo: {reason}</p>",
            TextBody = $"Agendamento Rejeitado. Seu agendamento com {providerName} foi rejeitado. Motivo: {reason}",
            CorrelationId = correlationId
        });

        var clientMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: clientPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(clientMessage, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Booking rejected notification enqueued for {BookingId}.", integrationEvent.BookingId);
        }
        catch (DbUpdateException ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(ex);

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping booking rejected notification for {BookingId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.BookingId, correlationId);
                return;
            }

            logger.LogError(ex, "Error enqueuing booking rejected notification for {BookingId}.", integrationEvent.BookingId);
            throw;
        }
    }
}
