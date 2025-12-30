using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar o evento de domínio quando um serviço é adicionado a um prestador.
/// </summary>
/// <remarks>
/// Publica um evento de integração para que outros módulos (como SearchProviders) possam reindexar o prestador.
/// </remarks>
public sealed class ProviderServiceAddedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<ProviderServiceAddedDomainEventHandler> logger) : IEventHandler<ProviderServiceAddedDomainEvent>
{
    public async Task HandleAsync(ProviderServiceAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling ProviderServiceAddedDomainEvent for provider {ProviderId}, service {ServiceId}",
                domainEvent.AggregateId,
                domainEvent.ServiceId);

            // Publica evento de integração para atualização do índice de busca
            var integrationEvent = new ProviderServicesUpdatedIntegrationEvent(
                Source: "Providers",
                ProviderId: domainEvent.AggregateId,
                ServiceIds: [] // O handler de integração vai reindexar o provider completo
            );

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation(
                "Published ProviderServicesUpdatedIntegrationEvent for provider {ProviderId}",
                domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling ProviderServiceAddedDomainEvent for provider {ProviderId}",
                domainEvent.AggregateId);

            // Não propagamos a exceção porque o serviço já foi adicionado com sucesso
            // A reindexação pode ser feita manualmente depois se necessário
        }
    }
}
