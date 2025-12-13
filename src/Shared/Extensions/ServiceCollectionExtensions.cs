using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Monitoring;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Seeding;
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

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona todos os serviços compartilhados da camada Shared.
    /// </summary>
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
            var mockEnvironment = new SimpleHostEnvironment(envName);
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

        // Adicionar seeding de dados de desenvolvimento
        services.AddDevelopmentSeeding();

        // Registra NoOpBackgroundJobService como implementação padrão
        // Módulos que precisam de Hangfire devem registrar HangfireBackgroundJobService explicitamente
        services.AddSingleton<IBackgroundJobService, NoOpBackgroundJobService>();

        return services;
    }

    public static IApplicationBuilder UseSharedServices(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseErrorHandling();
        app.UseAdvancedMonitoring();

        app.UseHangfireDashboardIfEnabled(configuration);

        return app;
    }

    public static async Task<IApplicationBuilder> UseSharedServicesAsync(this IApplicationBuilder app)
    {
        app.UseErrorHandling();
        // Nota: UseAdvancedMonitoring requer registro de BusinessMetrics durante a configuração de serviços.
        // O caminho assíncrono atualmente não registra esses serviços da mesma forma que o caminho síncrono.
        // TODO(#249): Align middleware registration between UseSharedServices() and UseSharedServicesAsync().
        // Issue: Async path skips BusinessMetrics registration causing UseAdvancedMonitoring to fail.
        // Solution: Extract shared middleware registration to ConfigureSharedMiddleware() method,
        // call from both paths, or conditionally apply monitoring based on IServiceCollection checks.
        // Impact: Development environments using async path lack business metrics dashboards.

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
            app.UseHangfireDashboardIfEnabled(configuration);

            // Garante que a infraestrutura de messaging seja criada (ignora em ambiente de teste ou quando desabilitado)
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
}
