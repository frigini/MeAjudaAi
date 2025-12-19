using Microsoft.Extensions.DependencyInjection;
namespace MeAjudaAi.Shared.Queries;

/// <summary>
/// Extension methods para configuração de Queries (CQRS)
/// </summary>
public static class QueriesExtensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        // Nota: Query handlers são registrados manualmente no método AddApplication() de cada módulo
        return services;
    }
}
