using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Modules.Users.API;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Logging;
using MeAjudaAi.ServiceDefaults;
using Serilog;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);
            ConfigureServices(builder);

            var app = builder.Build();

            ConfigureMiddleware(app);

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
                .Enrich.With<CorrelationIdEnricher>()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MeAjudaAi")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.With<CorrelationIdEnricher>()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}"));

            Log.Information("🚀 Iniciando MeAjudaAi API Service");
        }
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Configurações via ServiceDefaults e Shared (sem duplicar Serilog)
        builder.AddServiceDefaults();
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddApiServices(builder.Configuration, builder.Environment);
        builder.Services.AddUsersModule(builder.Configuration);
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.MapDefaultEndpoints();

        // Configurar serviços e módulos
        app.UseSharedServicesAsync().Wait();
        app.UseApiServices(app.Environment);
        app.UseUsersModule();
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
