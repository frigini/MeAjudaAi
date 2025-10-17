using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Commands;

public class CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger) : ICommandDispatcher
{
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        logger.LogInformation("Executing command {CommandType} with correlation {CorrelationId}",
            typeof(TCommand).Name, command.CorrelationId);

        await ExecuteWithPipeline(command, async () =>
        {
            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            await handler.HandleAsync(command, cancellationToken);
            return Unit.Value;
        }, cancellationToken);
    }

    public async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        logger.LogInformation("Executing command {CommandType} with correlation {CorrelationId}",
            typeof(TCommand).Name, command.CorrelationId);

        return await ExecuteWithPipeline(command, async () =>
        {
            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
            return await handler.HandleAsync(command, cancellationToken);
        }, cancellationToken);
    }

    private async Task<TResponse> ExecuteWithPipeline<TRequest, TResponse>(
        TRequest request,
        RequestHandlerDelegate<TResponse> handlerDelegate,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse();

        RequestHandlerDelegate<TResponse> pipeline = handlerDelegate;

        foreach (var behavior in behaviors)
        {
            var currentPipeline = pipeline;
            pipeline = () => behavior.Handle(request, currentPipeline, cancellationToken);
        }

        return await pipeline();
    }
}