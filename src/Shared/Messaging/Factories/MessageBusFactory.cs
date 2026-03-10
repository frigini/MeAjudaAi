using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Factories;

/// <summary>
/// Implementação do factory que seleciona o MessageBus baseado no ambiente:
/// - Default: RabbitMQ (se habilitado)
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
            try
            {
                _logger.LogInformation("Creating RabbitMQ MessageBus as default for environment: {Environment}", _environment.EnvironmentName);
                return _serviceProvider.GetRequiredService<RabbitMqMessageBus>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ MessageBus for environment {Environment}", _environment.EnvironmentName);
                throw new InvalidOperationException($"Failed to initialize RabbitMQ MessageBus for environment {_environment.EnvironmentName}", ex);
            }
        }
    }
}
