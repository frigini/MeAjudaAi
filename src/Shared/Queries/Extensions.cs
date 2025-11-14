using Microsoft.Extensions.DependencyInjection;
namespace MeAjudaAi.Shared.Queries;

internal static class Extensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        // Note: Query handlers are registered manually in each module's AddApplication() method
        return services;
    }
}
