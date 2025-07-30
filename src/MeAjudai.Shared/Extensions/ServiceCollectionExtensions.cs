using MeAjudai.Shared.Caching;
using MeAjudai.Shared.Commands;
using MeAjudai.Shared.Database;
using MeAjudai.Shared.Events;
using MeAjudai.Shared.Exceptions;
using MeAjudai.Shared.Messaging;
using MeAjudai.Shared.Queries;
using MeAjudai.Shared.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudai.Shared.Extensions;

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