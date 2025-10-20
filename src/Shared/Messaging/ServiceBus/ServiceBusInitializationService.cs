using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.ServiceBus;

internal class ServiceBusInitializationService(
    IServiceProvider serviceProvider,
    ILogger<ServiceBusInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Initializing Service Bus infrastructure...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var topicManager = scope.ServiceProvider.GetRequiredService<IServiceBusTopicManager>();

            await topicManager.EnsureTopicsExistAsync(cancellationToken);

            logger.LogInformation("Service Bus infrastructure initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize Service Bus infrastructure");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
