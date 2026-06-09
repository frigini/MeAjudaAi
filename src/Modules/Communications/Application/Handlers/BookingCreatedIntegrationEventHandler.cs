using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Users;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MeAjudaAi.Contracts.Enums;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

public sealed class BookingCreatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IProvidersModuleApi providersApi,
    IUsersModuleApi usersApi,
    ILogger<BookingCreatedIntegrationEventHandler> logger)
    : IEventHandler<BookingCreatedIntegrationEvent>
{
    private const string TemplateKey = "booking_created";

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

        var providerEmail = providerResult.Value.Email; // ModuleProviderDto has Email
        var clientEmail = clientResult.Value.Email;
        var clientName = clientResult.Value.FirstName;

        // Notificar o Provedor
        var providerPayload = JsonSerializer.Serialize(new
        {
            To = providerEmail,
            Subject = "Novo Agendamento Recebido",
            HtmlBody = $"<h1>Novo Agendamento!</h1><p>Você recebeu um novo agendamento de {clientName} para o dia {integrationEvent.Date}.</p>",
            TextBody = $"Novo Agendamento! Você recebeu um novo agendamento de {clientName} para o dia {integrationEvent.Date}.",
            CorrelationId = $"{correlationId}:provider",
            TemplateKey = TemplateKey
        });

        var providerMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: providerPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.High,
            correlationId: $"{correlationId}:provider");

        // Notificar o Cliente
        var clientPayload = JsonSerializer.Serialize(new
        {
            To = clientEmail,
            Subject = "Agendamento Solicitado",
            HtmlBody = $"<h1>Solicitação Enviada!</h1><p>Seu agendamento para o dia {integrationEvent.Date} foi enviado e aguarda confirmação do prestador.</p>",
            TextBody = $"Solicitação Enviada! Seu agendamento para o dia {integrationEvent.Date} foi enviado e aguarda confirmação do prestador.",
            CorrelationId = $"{correlationId}:client",
            TemplateKey = TemplateKey
        });

        var clientMessage = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: clientPayload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: $"{correlationId}:client");

        try
        {
            await outboxRepository.AddAsync(providerMessage, cancellationToken);
            await outboxRepository.AddAsync(clientMessage, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Booking created notifications enqueued for {BookingId}.",
                integrationEvent.BookingId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enqueuing booking created notifications for {BookingId}.", integrationEvent.BookingId);
        }
    }
}
