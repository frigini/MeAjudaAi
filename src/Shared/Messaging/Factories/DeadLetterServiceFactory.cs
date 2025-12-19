using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Factories;

/// <summary>
/// Implementação do factory que seleciona o serviço de DLQ baseado no ambiente:
/// - Development/Testing: Serviço RabbitMQ Dead Letter
/// - Production: Serviço Service Bus Dead Letter
/// </summary>
public sealed class DeadLetterServiceFactory(
    IServiceProvider serviceProvider,
    IHostEnvironment environment,
    ILogger<DeadLetterServiceFactory> logger) : IDeadLetterServiceFactory
{
    public IDeadLetterService CreateDeadLetterService()
    {
        if (environment.EnvironmentName == "Testing")
        {
            logger.LogInformation("Creating NoOp Dead Letter Service for Testing environment");
            return serviceProvider.GetRequiredService<NoOpDeadLetterService>();
        }
        else if (environment.IsDevelopment())
        {
            logger.LogInformation("Creating RabbitMQ Dead Letter Service for environment: {Environment}", environment.EnvironmentName);
            return serviceProvider.GetRequiredService<RabbitMqDeadLetterService>();
        }
        else
        {
            logger.LogInformation("Creating Service Bus Dead Letter Service for environment: {Environment}", environment.EnvironmentName);
            return serviceProvider.GetRequiredService<ServiceBusDeadLetterService>();
        }
    }
}
