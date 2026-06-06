using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar eventos de ativação de serviço.
/// </summary>
/// <remarks>
/// Quando um serviço é ativado, os prestadores que oferecem este serviço podem se tornar relevantes para novas buscas.
/// Para simplificar, poderíamos reindexar todos os prestadores desse serviço, mas aqui vamos focar na indexação sob demanda.
/// Se o serviço for novo, ele será indexado conforme os prestadores forem sendo atualizados.
/// </remarks>
public sealed class ServiceActivatedIntegrationEventHandler(
    ILogger<ServiceActivatedIntegrationEventHandler> logger) : IEventHandler<ServiceActivatedIntegrationEvent>
{
    public Task HandleAsync(ServiceActivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling ServiceActivatedIntegrationEvent for service {ServiceId} ({Name}).",
            integrationEvent.ServiceId,
            integrationEvent.Name);

        // NOTA: A indexação do SearchProviders é focada em Providers. 
        // Quando um serviço é ativado, ele passa a ser "filtrável".
        // Não precisamos fazer nada imediato no índice se a lógica de IndexProviderAsync já busca os serviços ativos.
        
        return Task.CompletedTask;
    }
}
