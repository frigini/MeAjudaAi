using MeAjudaAi.Shared.Messaging.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Implementação do factory que seleciona o serviço de DLQ baseado no ambiente:
/// - Development/Testing: Serviço RabbitMQ Dead Letter
/// - Production: Serviço Service Bus Dead Letter
/// </summary>
public sealed class EnvironmentBasedDeadLetterServiceFactory(
    IServiceProvider serviceProvider,
    IHostEnvironment environment,
    ILogger<EnvironmentBasedDeadLetterServiceFactory> logger) : IDeadLetterServiceFactory
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
