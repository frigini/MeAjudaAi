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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Web;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Handler para o evento de agendamento concluído.
/// Envia um convite para o cliente avaliar o prestador.
/// </summary>
public sealed class BookingCompletedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IProvidersModuleApi providersApi,
    IUsersModuleApi usersApi,
    IConfiguration configuration,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<BookingCompletedIntegrationEventHandler> logger)
    : IEventHandler<BookingCompletedIntegrationEvent>
{
    private const string TemplateKey = CommunicationTemplateKeys.BookingCompleted;

    public async Task HandleAsync(
        BookingCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.BookingId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping rating invite for {BookingId} — already sent.",
                integrationEvent.BookingId);
            return;
        }

        var providerResult = await providersApi.GetProviderByIdAsync(integrationEvent.ProviderId, cancellationToken);
        var clientResult = await usersApi.GetUserByIdAsync(integrationEvent.ClientId, cancellationToken);

        if (providerResult.IsFailure || providerResult.Value == null || clientResult.IsFailure || clientResult.Value == null)
        {
            logger.LogWarning("Could not find provider or client for rating invite notification.");
            return;
        }

        var providerName = HttpUtility.HtmlEncode(providerResult.Value.Name);
        var clientEmail = clientResult.Value.Email;
        var clientFirstName = HttpUtility.HtmlEncode(clientResult.Value.FirstName);

        var appBaseUrl = configuration[CommunicationConstants.ClientBaseUrlConfigKey];
        if (string.IsNullOrWhiteSpace(appBaseUrl))
        {
            throw new InvalidOperationException($"Configuration '{CommunicationConstants.ClientBaseUrlConfigKey}' is missing.");
        }
        var reviewUrl = $"{appBaseUrl.TrimEnd('/')}/reviews/create?bookingId={HttpUtility.UrlEncode(integrationEvent.BookingId.ToString())}";

        var payload = serializer.Serialize(new
        {
            To = clientEmail,
            Subject = $"Como foi sua experiência com {providerName}?",
            HtmlBody = $"<h1>Olá, {clientFirstName}!</h1><p>Seu agendamento com {providerName} foi concluído. Que tal deixar uma avaliação?</p><p><a href='{reviewUrl}'>Avaliar agora</a></p>",
            TextBody = $"Olá, {clientFirstName}! Seu agendamento com {providerName} foi concluído. Deixe sua avaliação em nosso app: {reviewUrl}",
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

            logger.LogInformation("Rating invite notification enqueued for {BookingId}.", integrationEvent.BookingId);
        }
        catch (Exception ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(
                ex as DbUpdateException ?? new DbUpdateException(ex.Message, ex));

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping rating invite for {BookingId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.BookingId, correlationId);
                return;
            }

            logger.LogError(ex, "Error enqueuing rating invite notification for {BookingId}.", integrationEvent.BookingId);
            throw;
        }
    }
}
