using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Factories;

/// <summary>
/// Implementação do factory que seleciona o MessageBus baseado no ambiente:
/// - Development/Testing: RabbitMQ (se habilitado)
/// - Production: Azure Service Bus
/// - Fallback: NoOpMessageBus para testes sem RabbitMQ
/// </summary>
public class MessageBusFactory : IMessageBusFactory
{
    private readonly IHostEnvironment _environment;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageBusFactory> _logger;

    public MessageBusFactory(
        IHostEnvironment environment,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<MessageBusFactory> logger)
    {
        _environment = environment;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public IMessageBus CreateMessageBus()
    {
        // Check if RabbitMQ is explicitly disabled
        var rabbitMqEnabled = _configuration.GetValue<bool?>("RabbitMQ:Enabled");

        if (_environment.IsDevelopment() || _environment.IsEnvironment(EnvironmentNames.Testing))
        {
            // Use RabbitMQ only if explicitly enabled or not configured (default behavior)
            if (rabbitMqEnabled != false)
            {
                try
                {
                    _logger.LogInformation("Creating RabbitMQ MessageBus for environment: {Environment}", _environment.EnvironmentName);
                    return _serviceProvider.GetRequiredService<RabbitMqMessageBus>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create RabbitMQ MessageBus, falling back to NoOp for testing");
                    return _serviceProvider.GetRequiredService<NoOpMessageBus>();
                }
            }
            else
            {
                _logger.LogInformation("RabbitMQ is disabled, using NoOp MessageBus for environment: {Environment}", _environment.EnvironmentName);
                return _serviceProvider.GetRequiredService<NoOpMessageBus>();
            }
        }
        else
        {
            _logger.LogInformation("Creating Azure Service Bus MessageBus for environment: {Environment}", _environment.EnvironmentName);
            return _serviceProvider.GetRequiredService<ServiceBusMessageBus>();
        }
    }
}
