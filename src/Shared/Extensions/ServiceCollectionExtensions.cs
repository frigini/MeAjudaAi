using Hangfire;
using Hangfire.PostgreSql;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common.Constants;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Monitoring;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Mock implementation of IHostEnvironment for cases where environment is not available
/// </summary>
internal class MockHostEnvironment : IHostEnvironment
{
    public MockHostEnvironment(string environmentName)
    {
        EnvironmentName = environmentName;
        ApplicationName = "MeAjudaAi";
        ContentRootPath = Directory.GetCurrentDirectory();
    }

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

public static class ServiceCollectionExtensions
{
    [Obsolete("Use AddSharedServices with IHostEnvironment parameter instead")]
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddCustomSerialization();
        // Serilog configurado no Program.cs do ApiService

        services.AddPostgres(configuration);
        services.AddCaching(configuration);

        // Só adiciona messaging se não estiver em ambiente de teste
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                     Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                     EnvironmentNames.Development;
        var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");

        var isTestingEnvironment = envName == EnvironmentNames.Testing ||
                                 envName.Equals("Testing", StringComparison.OrdinalIgnoreCase) ||
                                 integrationTests == "true" ||
                                 integrationTests == "1";

        if (!isTestingEnvironment)
        {
            // Cria um mock environment baseado na variável de ambiente
            var mockEnvironment = new MockHostEnvironment(envName);
            services.AddMessaging(configuration, mockEnvironment);
        }
        else
        {
            // Registra messaging no-op para testes
            services.AddSingleton<IMessageBus, NoOpMessageBus>();
            services.AddSingleton<MeAjudaAi.Shared.Messaging.ServiceBus.IServiceBusTopicManager, NoOpServiceBusTopicManager>();
        }

        services.AddValidation();
        services.AddErrorHandling();

        services.AddCommands();
        services.AddQueries();
        services.AddEvents();

        return services;
    }

    [Obsolete("Use AddCoreSharedServices, AddInfrastructureServices, and AddBackgroundJobs instead")]
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Cast para IWebHostEnvironment se possível, senão usar apenas a configuração básica
        if (environment is IWebHostEnvironment webHostEnv)
        {
            services.AddSharedServices(configuration, webHostEnv);
        }
        else
        {
            // Fallback para configuração básica sem IWebHostEnvironment
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddCustomSerialization();
            services.AddPostgres(configuration);
            services.AddCaching(configuration);

            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? EnvironmentNames.Development;
            if (envName != EnvironmentNames.Testing)
            {
                var mockEnvironment = new MockHostEnvironment(envName);
                services.AddMessaging(configuration, mockEnvironment);
            }
            else
            {
                services.AddSingleton<IMessageBus, NoOpMessageBus>();
                services.AddSingleton<MeAjudaAi.Shared.Messaging.ServiceBus.IServiceBusTopicManager, NoOpServiceBusTopicManager>();
            }

            services.AddValidation();
            services.AddErrorHandling();
            services.AddCommands();
            services.AddQueries();
            services.AddEvents();
        }

        // Adiciona monitoramento avançado complementar ao Aspire
        services.AddAdvancedMonitoring(environment);

        return services;
    }

    private static void ConfigureHangfireDashboard(IApplicationBuilder app, IConfiguration configuration)
    {
        var dashboardEnabled = configuration.GetValue<bool>("Hangfire:DashboardEnabled", false);
        if (dashboardEnabled)
        {
            var dashboardPath = configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
            app.UseHangfireDashboard(dashboardPath, new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                StatsPollingInterval = 5000,
                DisplayStorageConnectionString = false
            });
        }
    }

    public static IApplicationBuilder UseSharedServices(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseErrorHandling();
        app.UseAdvancedMonitoring();

        ConfigureHangfireDashboard(app, configuration);

        return app;
    }

    public static async Task<IApplicationBuilder> UseSharedServicesAsync(this IApplicationBuilder app)
    {
        app.UseErrorHandling();
        // Note: UseAdvancedMonitoring requires BusinessMetrics registration during service configuration.
        // The async path doesn't currently register these services in the same way as the sync path.
        // TODO: Align middleware registration between sync/async paths or conditionally apply monitoring.

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                         Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                         "Development";
        var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");

        var isTestingEnvironment = environment == "Testing" ||
                                 environment.Equals("Testing", StringComparison.OrdinalIgnoreCase) ||
                                 integrationTests == "true" ||
                                 integrationTests == "1";

        if (app is WebApplication webApp)
        {
            var configuration = webApp.Services.GetRequiredService<IConfiguration>();

            // Configurar Hangfire Dashboard se habilitado
            ConfigureHangfireDashboard(app, configuration);

            // Garante que a infraestrutura de messaging seja criada (ignora em ambiente de teste ou quando desabilitado)
            if (!isTestingEnvironment)
            {
                var isMessagingEnabled = configuration.GetValue<bool>("Messaging:Enabled", true);

                if (isMessagingEnabled)
                {
                    await webApp.EnsureMessagingInfrastructureAsync();
                }

                // Cache warmup em background para não bloquear startup
                var isCacheWarmupEnabled = configuration.GetValue<bool>("Cache:WarmupEnabled", true);
                if (isCacheWarmupEnabled)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = webApp.Services.CreateScope();
                            var warmupService = scope.ServiceProvider.GetService<ICacheWarmupService>();
                            if (warmupService != null)
                            {
                                await warmupService.WarmupAsync();
                            }
                            else
                            {
                                var logger = webApp.Services.GetService<ILogger<ICacheWarmupService>>();
                                logger?.LogDebug("ICacheWarmupService não registrado - esperado em ambientes de teste");
                            }
                        }
                        catch (Exception ex)
                        {
                            var logger = webApp.Services.GetRequiredService<ILogger<ICacheWarmupService>>();
                            logger.LogWarning(ex, "Falha ao aquecer o cache durante a inicialização - pode ser esperado em testes");
                        }
                    });
                }
            }
        }

        return app;
    }


}
