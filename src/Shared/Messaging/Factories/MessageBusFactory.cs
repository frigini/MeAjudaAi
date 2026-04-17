using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.Rebus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Factories;

/// <summary>
/// Implementação do factory que seleciona o MessageBus baseado no ambiente:
/// - Default: Rebus (se habilitado)
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
        // Verificar se o serviço de Mensageria está habilitado
        var isEnabled = _configuration.GetValue<bool>("Messaging:Enabled", true);

        if (_environment.IsEnvironment(EnvironmentNames.Testing) || !isEnabled)
        {
            return _serviceProvider.GetRequiredService<NoOpMessageBus>();
        }

        try
        {
            _logger.LogInformation("Creating Rebus MessageBus for environment: {Environment}", _environment.EnvironmentName);
            return _serviceProvider.GetRequiredService<RebusMessageBus>();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex.GetType().Name.Contains("ServiceActivationException"))
        {
            _logger.LogError(ex, "Failed to initialize Rebus MessageBus for environment {Environment}", _environment.EnvironmentName);
            throw new InvalidOperationException($"Failed to initialize Rebus MessageBus for environment {_environment.EnvironmentName}", ex);
        }
    }
}
