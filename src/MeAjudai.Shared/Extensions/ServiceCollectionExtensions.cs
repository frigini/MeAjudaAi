using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLogging();
        services.AddErrorHandling();
        services.AddCommands();
        services.AddQueries();
        services.AddEvents();
        services.AddCaching(configuration);
        services.AddMessaging(configuration);
        services.AddPostgres(configuration);
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    public static IApplicationBuilder UseSharedServices(this IApplicationBuilder app)
    {
        app.UseErrorHandling();

        return app;
    }
}