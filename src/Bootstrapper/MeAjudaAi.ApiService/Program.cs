using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Modules.Users.API;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.ServiceDefaults;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 🚀 Configurar Serilog apenas se NÃO for ambiente de Testing
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
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

        Log.Information("🚀 Iniciando MeAjudaAi API Service");
    }

    // Configurações via ServiceDefaults e Shared (sem duplicar Serilog)
    builder.AddServiceDefaults();
    builder.Services.AddSharedServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration, builder.Environment);
    builder.Services.AddUsersModule(builder.Configuration);

    var app = builder.Build();

    app.MapDefaultEndpoints();

    // Configurar serviços e módulos
    await app.UseSharedServicesAsync();
    app.UseApiServices(app.Environment);
    app.UseUsersModule();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        var environmentName = app.Environment.IsEnvironment("Integration") ? "Integration Test" : app.Environment.EnvironmentName;
        Log.Information("✅ MeAjudaAi API Service configurado com sucesso - Ambiente: {Environment}", environmentName);
    }

    app.Run();
}
catch (Exception ex)
{
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
    {
        Log.Fatal(ex, "❌ Falha crítica ao inicializar MeAjudaAi API Service");
    }
    throw;
}

// Make Program class accessible for integration tests
public partial class Program { }