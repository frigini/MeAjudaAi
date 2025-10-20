using Microsoft.Extensions.DependencyInjection;
namespace MeAjudaAi.Shared.Queries;

internal static class Extensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

        services.Scan(scan => scan
            .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
