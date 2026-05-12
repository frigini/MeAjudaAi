using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.ApiService.Endpoints;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.Modules.Communications.API;
using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Locations.API;
using MeAjudaAi.Modules.Providers.API;
using MeAjudaAi.Modules.Ratings.API;
using MeAjudaAi.Modules.Payments.API;
using MeAjudaAi.Modules.Bookings.API;
using MeAjudaAi.Modules.SearchProviders.API;
using MeAjudaAi.Modules.ServiceCatalogs.API;
using MeAjudaAi.Modules.Users.API;
using MeAjudaAi.ServiceDefaults;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Logging;
using MeAjudaAi.Shared.Seeding;
using Microsoft.FeatureManagement;
using Serilog;
using Serilog.Context;

namespace MeAjudaAi.ApiService;

[ExcludeFromCodeCoverage]
public partial class Program
{
    static Program()
    {
        var diagPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "program_static_diag.log");
        try 
        { 
            System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] Program static constructor called.{System.Environment.NewLine}"); 
        } 
        catch { /* Best effort, ignore failures to prevent startup abortion */ }
    }

    protected Program() { }

    public static async Task Main(string[] args)
    {
        // Correção para compatibilidade DateTime UTC com PostgreSQL timestamp
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(opts => opts.AddServerHeader = false);

            ConfigureLogging(builder);

            // Configurações via ServiceDefaults e Shared (sem duplicar Serilog)
            builder.AddServiceDefaults();
            builder.Services.AddHttpContextAccessor();

            var diagPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "program_diag.log");
            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] Program.Main starting...{System.Environment.NewLine}"); } catch { }

            // Registrar módulos ANTES de AddSharedServices
            // (exception handlers específicos devem ser registrados antes do global)
            builder.Services.AddUsersModule(builder.Configuration);
            builder.Services.AddProvidersModule(builder.Configuration);
            builder.Services.AddDocumentsModule(builder.Configuration, builder.Environment);
            builder.Services.AddSearchProvidersModule(builder.Configuration, builder.Environment);
            builder.Services.AddLocationsModule(builder.Configuration);
            builder.Services.AddServiceCatalogsModule(builder.Configuration);
            builder.Services.AddCommunicationsModule(builder.Configuration);
            builder.Services.AddRatingsModule(builder.Configuration, builder.Environment);
            builder.Services.AddPaymentsModule(builder.Configuration, builder.Environment);
            builder.Services.AddBookingsModule(builder.Configuration, builder.Environment);
            
            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] Modules added. Adding SharedServices...{System.Environment.NewLine}"); } catch { }

            // Shared services por último (GlobalExceptionHandler atua como fallback)
            builder.Services.AddSharedServices(builder.Configuration);
            builder.Services.AddApiServices(builder.Configuration, builder.Environment);

            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] SharedServices added.{System.Environment.NewLine}"); } catch { }

            if (builder.Environment.IsEnvironment("Testing") || builder.Environment.IsEnvironment("Integration"))
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.SetIsOriginAllowed(_ => true)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                });
            }
            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] Adding FeatureManagement...{System.Environment.NewLine}"); } catch { }
            builder.Services.AddFeatureManagement();
            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] FeatureManagement added.{System.Environment.NewLine}"); } catch { }

            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] Building app...{System.Environment.NewLine}"); } catch { }
            var app = builder.Build();
            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] App built. Configuring middleware...{System.Environment.NewLine}"); } catch { }

            await ConfigureMiddlewareAsync(app);
            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] Middleware configured.{System.Environment.NewLine}"); } catch { }

            // Aplicar migrations...
            // ...
            
            try { System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] Startup complete. Checking if we should run app...{System.Environment.NewLine}"); } catch { }
            LogStartupComplete(app);

            if (!app.Environment.IsEnvironment("Testing"))
            {
                await app.RunAsync();
            }
            else
            {
                // Em ambiente de teste, StartAsync inicia sem bloquear, e WaitForShutdownAsync garante que o host permaneça estável.
                await app.StartAsync();
                await app.WaitForShutdownAsync();
            }
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
            builder.Logging.SetMinimumLevel(LogLevel.Information);
        }
    }

    private static async Task ConfigureMiddlewareAsync(WebApplication app)
    {
        if (app.Environment.IsEnvironment("Testing") || app.Environment.IsEnvironment("Integration"))
        {
            app.UseCors("AllowAll");
        }

        app.MapDefaultEndpoints();
        // Configurar serviços e módulos
        await app.UseSharedServicesAsync();
        app.UseApiServices(app.Environment);
        app.UseUsersModule();
        app.UseProvidersModule();
        app.UseDocumentsModule();
        app.UseSearchProvidersModule();
        app.UseLocationsModule();
        app.UseServiceCatalogsModule();
        app.UseCommunicationsModule();
        app.UseRatingsModule();
        app.UsePaymentsModule();
        app.UseBookingsModule();

        // Endpoints de orquestração cross-módulo (ficam no ApiService)
        app.MapProviderRegistrationEndpoints();
        app.MapCommunicationsEndpoints();
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
