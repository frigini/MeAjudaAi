using System.Diagnostics;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Modules.Catalogs.API;
using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Location.Infrastructure;
using MeAjudaAi.Modules.Providers.API;
using MeAjudaAi.Modules.SearchProviders.API;
using MeAjudaAi.Modules.Users.API;
using MeAjudaAi.ServiceDefaults;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Logging;
using Serilog;
using Serilog.Context;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);

            // Configurações via ServiceDefaults e Shared (sem duplicar Serilog)
            builder.AddServiceDefaults();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSharedServices(builder.Configuration);
            builder.Services.AddApiServices(builder.Configuration, builder.Environment);

            // Registrar módulos
            builder.Services.AddUsersModule(builder.Configuration);
            builder.Services.AddProvidersModule(builder.Configuration);
            builder.Services.AddDocumentsModule(builder.Configuration);
            builder.Services.AddSearchModule(builder.Configuration);
            builder.Services.AddLocationModule(builder.Configuration);
            builder.Services.AddCatalogsModule(builder.Configuration);

            var app = builder.Build();

            await ConfigureMiddlewareAsync(app);

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
            // Bootstrap logger for early startup messages
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

            Log.Information("🚀 Iniciando MeAjudaAi API Service");
        }
        else
        {
            // For Testing environment, use minimal console logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Warning);
        }
    }

    private static async Task ConfigureMiddlewareAsync(WebApplication app)
    {
        app.MapDefaultEndpoints();

        // Add structured logging middleware (will conditionally add Serilog request logging based on environment)
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
        app.UseSearchModule();
        app.UseLocationModule();
        app.UseCatalogsModule();
    }

    private static void LogStartupComplete(WebApplication app)
    {
        if (!app.Environment.IsEnvironment("Testing"))
        {
            var environmentName = app.Environment.IsEnvironment("Integration") ? "Integration Test" : app.Environment.EnvironmentName;
            Log.Information("✅ MeAjudaAi API Service configurado com sucesso - Ambiente: {Environment}", environmentName);
        }
    }

    private static void HandleStartupException(Exception ex)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
        {
            Log.Fatal(ex, "❌ Falha crítica ao inicializar MeAjudaAi API Service");
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
