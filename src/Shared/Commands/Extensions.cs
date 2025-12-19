using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Commands;

internal static class Extensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        // Note: Command handlers are registered manually in each module's AddApplication() method
        return services;
    }
}
