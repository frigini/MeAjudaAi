using Microsoft.Extensions.DependencyInjection;
namespace MeAjudaAi.Shared.Queries;

internal static class Extensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        // Nota: Query handlers são registrados manualmente no método AddApplication() de cada módulo
        return services;
    }
}
