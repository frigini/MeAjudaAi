using MeAjudaAi.Shared.Caching;
using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Monitoring;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Seeding;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Streaming;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona todos os serviços compartilhados da camada Shared.
    /// </summary>
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddCustomSerialization();
        // Serilog configurado no Program.cs do ApiService

        services.AddPostgres(configuration);
        services.AddDapper();
        services.AddCaching(configuration);

        // Só adiciona messaging se não estiver em ambiente de teste
        if (!environment.IsEnvironment(EnvironmentNames.Testing))
        {
            services.AddMessaging(configuration, environment);
        }
        else
        {
            // Registra messaging no-op para testes
            services.AddSingleton<IMessageBus, NoOpMessageBus>();
        }

        services.AddValidation();
        services.AddErrorHandling();

        services.AddCommands();
        services.AddQueries();
        services.AddEvents();

        // Adicionar seeding de dados de desenvolvimento
        services.AddDevelopmentSeeding();

        // Adicionar Business Metrics (necessário para UseAdvancedMonitoring)
        services.AddBusinessMetrics();

        // Registra NoOpBackgroundJobService como implementação padrão
        // Módulos que precisam de Hangfire devem registrar HangfireBackgroundJobService explicitamente
        services.AddSingleton<IBackgroundJobService, NoOpBackgroundJobService>();

        // Streaming services (SSE)
        services.AddStreaming();

        return services;
    }

    public static IApplicationBuilder UseSharedServices(this IApplicationBuilder app, IConfiguration configuration)
    {
        ConfigureSharedMiddleware(app, configuration);
        return app;
    }

    public static async Task<IApplicationBuilder> UseSharedServicesAsync(this IApplicationBuilder app, IHostEnvironment environment)
    {
        if (app is WebApplication webApp)
        {
            var configuration = webApp.Services.GetRequiredService<IConfiguration>();
            var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");

            var isTestingEnvironment = environment.IsEnvironment(EnvironmentNames.Testing) ||
                                     integrationTests == "true" ||
                                     integrationTests == "1";

            // Configure shared middleware (error handling, monitoring, Hangfire)
            ConfigureSharedMiddleware(app, configuration);

            // Ensure messaging infrastructure is created (skip in test environment or when disabled)
            if (!isTestingEnvironment)
            {
                var isMessagingEnabled = configuration.GetValue<bool>("Messaging:Enabled", true);

                if (isMessagingEnabled)
                {
                    await webApp.EnsureMessagingInfrastructureAsync();
                }
            }
        }

        return app;
    }

    /// <summary>
    /// Configures shared middleware for both synchronous and asynchronous initialization paths.
    /// Ensures consistent middleware registration including error handling, advanced monitoring, and Hangfire dashboard.
    /// </summary>
    private static void ConfigureSharedMiddleware(IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseErrorHandling();
        app.UseAdvancedMonitoring();
        app.UseHangfireDashboardIfEnabled(configuration);
    }
    public static IServiceCollection AddStreaming(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ISseHub<>), typeof(SseHub<>));
        return services;
    }
}
