using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Commands;

public class CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger) : ICommandDispatcher
{
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        logger.LogInformation("Executing command {CommandType} with correlation {CorrelationId}",
            typeof(TCommand).Name, command.CorrelationId);

        await handler.HandleAsync(command, cancellationToken);
    }

    public async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        logger.LogInformation("Executing command {CommandType} with correlation {CorrelationId}",
            typeof(TCommand).Name, command.CorrelationId);

        return await handler.HandleAsync(command, cancellationToken);
    }
}