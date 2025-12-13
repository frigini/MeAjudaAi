using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio ProviderRegisteredDomainEvent e publica eventos de integração.
/// </summary>
/// <remarks>
/// Responsável por converter eventos de domínio em eventos de integração para comunicação
/// entre módulos. Quando um prestador é registrado no domínio, este handler busca os dados
/// atualizados e publica um evento de integração para notificar outros sistemas.
/// </remarks>
public sealed class ProviderRegisteredDomainEventHandler(
    IMessageBus messageBus,
    ProvidersDbContext context,
    ILogger<ProviderRegisteredDomainEventHandler> logger) : IEventHandler<ProviderRegisteredDomainEvent>
{
    /// <summary>
    /// Processa o evento de domínio de prestador registrado de forma assíncrona.
    /// </summary>
    /// <param name="domainEvent">Evento de domínio contendo dados do prestador registrado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task HandleAsync(ProviderRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ProviderRegisteredDomainEvent for provider {ProviderId}", domainEvent.AggregateId);

            // Busca o prestador para garantir que temos os dados mais recentes
            var provider = await context.Providers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == new Domain.ValueObjects.ProviderId(domainEvent.AggregateId), cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found when handling ProviderRegisteredDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Cria evento de integração para sistemas externos usando mapper
            var integrationEvent = domainEvent.ToIntegrationEvent();

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published ProviderRegistered integration event for provider {ProviderId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProviderRegisteredDomainEvent for provider {ProviderId}", domainEvent.AggregateId);
            throw new InvalidOperationException(
                $"Failed to publish ProviderRegistered integration event for provider '{domainEvent.AggregateId}'",
                ex);
        }
    }
}
