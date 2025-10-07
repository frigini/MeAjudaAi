using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Mappers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio UserRegisteredDomainEvent e publica eventos de integração UserRegisteredIntegrationEvent.
/// </summary>
/// <remarks>
/// Responsável por converter eventos de domínio em eventos de integração para comunicação
/// entre módulos. Quando um usuário é registrado no domínio, este handler busca os dados
/// atualizados e publica um evento de integração para notificar outros sistemas.
/// </remarks>
internal sealed class UserRegisteredDomainEventHandler(
    IMessageBus messageBus,
    UsersDbContext context,
    ILogger<UserRegisteredDomainEventHandler> logger) : IEventHandler<UserRegisteredDomainEvent>
{
    /// <summary>
    /// Processa o evento de domínio de usuário registrado de forma assíncrona.
    /// </summary>
    /// <param name="domainEvent">Evento de domínio contendo dados do usuário registrado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task HandleAsync(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling UserRegisteredDomainEvent for user {UserId}", domainEvent.AggregateId);

            // Busca o usuário para garantir que temos os dados mais recentes
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == new Domain.ValueObjects.UserId(domainEvent.AggregateId), cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User {UserId} not found when handling UserRegisteredDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Cria evento de integração para sistemas externos usando mapper
            var baseEvent = domainEvent.ToIntegrationEvent();
            var integrationEvent = baseEvent with
            {
                KeycloakId = user.KeycloakId ?? string.Empty, // Será definido após criação no Keycloak
                Roles = ["customer"] // Papel padrão
            };

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published UserRegistered integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling UserRegisteredDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw;
        }
    }
}
