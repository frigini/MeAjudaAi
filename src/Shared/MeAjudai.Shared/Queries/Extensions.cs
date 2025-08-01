using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Queries;

internal static class Extensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(IQuery<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}