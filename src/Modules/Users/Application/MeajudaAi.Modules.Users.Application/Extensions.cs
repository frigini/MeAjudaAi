using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application layer only contains interfaces and DTOs
        // Actual service implementations are in Infrastructure layer to avoid circular dependencies
        // Domain event handlers are automatically registered by the shared Events extension
        
        return services;
    }
}
