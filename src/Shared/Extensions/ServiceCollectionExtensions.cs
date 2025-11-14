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

        // Background Jobs com Hangfire
        services.AddHangfireJobs(configuration);

        return services;
    }

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

            // Background Jobs com Hangfire
            services.AddHangfireJobs(configuration);
        }

        // Adiciona monitoramento avançado complementar ao Aspire
        services.AddAdvancedMonitoring(environment);

        return services;
    }

    public static IApplicationBuilder UseSharedServices(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseErrorHandling();
        app.UseAdvancedMonitoring(); // Adiciona middleware de métricas

        // Hangfire Dashboard (disponível em /hangfire) - apenas se explicitamente habilitado
        var dashboardEnabled = configuration.GetValue<bool>("Hangfire:DashboardEnabled", false);
        if (dashboardEnabled)
        {
            var dashboardPath = configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
            app.UseHangfireDashboard(dashboardPath, new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                StatsPollingInterval = 5000, // 5 segundos
                DisplayStorageConnectionString = false
            });
        }

        return app;
    }

    public static async Task<IApplicationBuilder> UseSharedServicesAsync(this IApplicationBuilder app)
    {
        app.UseErrorHandling();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                         Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                         "Development";
        var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");

        var isTestingEnvironment = environment == "Testing" ||
                                 environment.Equals("Testing", StringComparison.OrdinalIgnoreCase) ||
                                 integrationTests == "true" ||
                                 integrationTests == "1";

        // Configurar Hangfire Dashboard se habilitado
        if (app is WebApplication webApp)
        {
            var configuration = webApp.Services.GetRequiredService<IConfiguration>();
            var dashboardEnabled = configuration.GetValue<bool>("Hangfire:DashboardEnabled", false);
            
            if (dashboardEnabled)
            {
                var dashboardPath = configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
                app.UseHangfireDashboard(dashboardPath, new DashboardOptions
                {
                    Authorization = new[] { new HangfireAuthorizationFilter() },
                    StatsPollingInterval = 5000, // 5 segundos
                    DisplayStorageConnectionString = false
                });
            }
        }

        // Garante que a infraestrutura de messaging seja criada (ignora em ambiente de teste ou quando desabilitado)
        if (app is WebApplication webApp2 && !isTestingEnvironment)
        {
            var configuration = webApp2.Services.GetRequiredService<IConfiguration>();
            var isMessagingEnabled = configuration.GetValue<bool>("Messaging:Enabled", true);

            if (isMessagingEnabled)
            {
                await webApp2.EnsureMessagingInfrastructureAsync();
            }

            // Cache warmup em background para não bloquear startup
            var isCacheWarmupEnabled = configuration.GetValue<bool>("Cache:WarmupEnabled", true);
            if (isCacheWarmupEnabled)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = webApp2.Services.CreateScope();
                        var warmupService = scope.ServiceProvider.GetService<ICacheWarmupService>();
                        if (warmupService != null)
                        {
                            await warmupService.WarmupAsync();
                        }
                        else
                        {
                            var logger = webApp2.Services.GetService<ILogger<ICacheWarmupService>>();
                            logger?.LogDebug("ICacheWarmupService não registrado - esperado em ambientes de teste");
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = webApp2.Services.GetRequiredService<ILogger<ICacheWarmupService>>();
                        logger.LogWarning(ex, "Falha ao aquecer o cache durante a inicialização - pode ser esperado em testes");
                    }
                });
            }
        }

        return app;
    }

    [Obsolete]
    private static IServiceCollection AddHangfireJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // Desabilitar Hangfire em ambientes de teste ou quando explicitamente configurado
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                         Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        
        var isTestingEnvironment = environment == "Testing" ||
                                 environment?.Equals("Testing", StringComparison.OrdinalIgnoreCase) == true ||
                                 integrationTests == "true" ||
                                 integrationTests == "1";
        
        var hangfireEnabled = configuration.GetValue<bool>("Hangfire:Enabled", true);
        
        // Usar mesma cadeia de resolução de connection string que AddPostgres para evitar falhas em ambientes válidos
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? configuration.GetConnectionString("meajudaai-db-local")
                              ?? configuration.GetConnectionString("meajudaai-db")
                              ?? configuration["Postgres:ConnectionString"];
        
        // Se não há connection string, desabilitar Hangfire automaticamente
        if (!hangfireEnabled || isTestingEnvironment || string.IsNullOrEmpty(connectionString))
        {
            // Registrar implementação nula para IBackgroundJobService quando Hangfire está desabilitado
            services.AddSingleton<IBackgroundJobService, NoOpBackgroundJobService>();
            return services;
        }

        // Ler configurações do Hangfire do IConfiguration ao invés de hardcode
        var workerCount = configuration.GetValue<int?>("Hangfire:WorkerCount") ?? Environment.ProcessorCount * 2;
        var pollingIntervalSeconds = configuration.GetValue<int>("Hangfire:PollingIntervalSeconds", 15);

        // Configura Hangfire com PostgreSQL
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
            {
                SchemaName = "hangfire",
                QueuePollInterval = TimeSpan.FromSeconds(pollingIntervalSeconds),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                InvisibilityTimeout = TimeSpan.FromMinutes(30)
            }));

        // Adiciona servidor Hangfire para processar jobs
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = workerCount;
            options.Queues = new[] { "default", "critical", "low" };
            options.ServerName = $"{Environment.MachineName}-{Guid.NewGuid().ToString()[..8]}";
        });

        // Registra nosso wrapper do Hangfire
        services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();

        return services;
    }
}
