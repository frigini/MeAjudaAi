using System.Diagnostics;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Locations.API;
using MeAjudaAi.Modules.Providers.API;
using MeAjudaAi.Modules.SearchProviders.API;
using MeAjudaAi.Modules.ServiceCatalogs.API;
using MeAjudaAi.Modules.Users.API;
using MeAjudaAi.ServiceDefaults;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Logging;
using MeAjudaAi.Shared.Seeding;
using Serilog;
using Serilog.Context;

namespace MeAjudaAi.ApiService;

public partial class Program
{
    protected Program() { }

    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);

            // Configurações via ServiceDefaults e Shared (sem duplicar Serilog)
            builder.AddServiceDefaults();
            builder.Services.AddHttpContextAccessor();

            // Registrar módulos ANTES de AddSharedServices
            // (exception handlers específicos devem ser registrados antes do global)
            builder.Services.AddUsersModule(builder.Configuration);
            builder.Services.AddProvidersModule(builder.Configuration);
            builder.Services.AddDocumentsModule(builder.Configuration);
            builder.Services.AddSearchProvidersModule(builder.Configuration, builder.Environment);
            builder.Services.AddLocationsModule(builder.Configuration);
            builder.Services.AddServiceCatalogsModule(builder.Configuration);

            // Shared services por último (GlobalExceptionHandler atua como fallback)
            builder.Services.AddSharedServices(builder.Configuration);
            builder.Services.AddApiServices(builder.Configuration, builder.Environment);
            builder.Services.AddCustomRateLimiting();

            var app = builder.Build();

            await ConfigureMiddlewareAsync(app);

            // Aplicar migrations de todos os módulos ANTES de seed
            // Pular em ambiente de Testing pois os testes controlam suas próprias migrations
            if (!app.Environment.IsEnvironment("Testing"))
            {
                await app.ApplyModuleMigrationsAsync();
                
                // Seed de dados de desenvolvimento
                await app.SeedDevelopmentDataIfNeededAsync();
            }

            LogStartupComplete(app);

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            HandleStartupException(ex);
            throw;
        }
        finally
        {
            CloseLogging();
        }
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        // Configurar Serilog apenas se NÃO for ambiente de Testing
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            // Logger de inicialização para mensagens de startup precoces
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MeAjudaAi")
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MeAjudaAi")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"),
                writeToProviders: false, preserveStaticLogger: false);

            Log.Information("🚀 Starting MeAjudaAi API Service");
        }
        else
        {
            // Para ambiente de Testing, usa logging mínimo no console
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Warning);
        }
    }

    private static async Task ConfigureMiddlewareAsync(WebApplication app)
    {
        app.MapDefaultEndpoints();
        app.UseRateLimiter();

        // Adiciona middleware de logging estruturado (condicionalmente adiciona Serilog request logging baseado no ambiente)
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseStructuredLogging();
        }

        // Configurar serviços e módulos
        await app.UseSharedServicesAsync();
        app.UseApiServices(app.Environment);
        app.UseUsersModule();
        app.UseProvidersModule();
        app.UseDocumentsModule();
        app.UseSearchProvidersModule();
        app.UseLocationsModule();
        app.UseServiceCatalogsModule();
    }

    private static void LogStartupComplete(WebApplication app)
    {
        if (!app.Environment.IsEnvironment("Testing"))
        {
            var environmentName = app.Environment.IsEnvironment("Integration") ? "Integration Test" : app.Environment.EnvironmentName;
            Log.Information("✅ MeAjudaAi API Service configured successfully - Environment: {Environment}", environmentName);
        }
    }

    private static void HandleStartupException(Exception ex)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
        {
            Log.Fatal(ex, "❌ Critical failure initializing MeAjudaAi API Service");
        }
    }

    private static void CloseLogging()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
        {
            Log.CloseAndFlush();
        }
    }
}
