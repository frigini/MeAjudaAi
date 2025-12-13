using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio ProviderActivatedDomainEvent e publica eventos de integração.
/// </summary>
/// <remarks>
/// Responsável por converter eventos de domínio em eventos de integração para comunicação
/// entre módulos. Quando um prestador é ativado no sistema, este handler publica um evento
/// de integração para notificar outros sistemas, como o módulo de busca.
/// </remarks>
public sealed class ProviderActivatedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<ProviderActivatedDomainEventHandler> logger) : IEventHandler<ProviderActivatedDomainEvent>
{
    /// <summary>
    /// Processa o evento de domínio de prestador ativado de forma assíncrona.
    /// </summary>
    /// <param name="domainEvent">Evento de domínio contendo dados do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task HandleAsync(ProviderActivatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ProviderActivatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);

            // Cria evento de integração para sistemas externos usando mapper
            var integrationEvent = domainEvent.ToIntegrationEvent();

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published ProviderActivated integration event for provider {ProviderId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProviderActivatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);
            throw new InvalidOperationException(
                $"Failed to publish ProviderActivated integration event for provider '{domainEvent.AggregateId}'",
                ex);
        }
    }
}
