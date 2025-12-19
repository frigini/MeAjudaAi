using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Commands;

/// <summary>
/// Extension methods para configuração de Commands (CQRS)
/// </summary>
public static class CommandsExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        // Note: Command handlers are registered manually in each module's AddApplication() method
        return services;
    }
}
