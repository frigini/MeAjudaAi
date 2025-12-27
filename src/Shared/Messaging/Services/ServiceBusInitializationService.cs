using MeAjudaAi.Shared.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Services;

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
            throw new InvalidOperationException(
                "Failed to initialize Azure Service Bus infrastructure (topics, subscriptions, and admin client)",
                ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
