using MeAjudaAi.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

// Simplified environment detection
var isTesting = builder.Environment.EnvironmentName == "Testing";

Console.WriteLine($"[AppHost] Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"[AppHost] IsTesting: {isTesting}");

if (isTesting)
{
    // Testing environment - minimal setup for faster tests
    Console.WriteLine("[AppHost] Configurando ambiente de teste simplificado...");
    
    var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
    {
        options.IsTestEnvironment = true;
        options.MainDatabase = "meajudaai";
        options.Username = "postgres";
        options.Password = "dev123";
    });

    // TODO: Redis configuration simplificada temporariamente
    var redis = builder.AddRedis("redis");

    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference((IResourceBuilder<IResourceWithConnectionString>)postgresql.MainDatabase, "DefaultConnection")
        .WithReference(redis)
        .WaitFor(redis)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Testing")
        .WithEnvironment("Logging:LogLevel:Default", "Information")
        .WithEnvironment("Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning")
        .WithEnvironment("Logging:LogLevel:Microsoft.Hosting.Lifetime", "Information")
        // Desabilitar features que podem causar problemas em testes
        .WithEnvironment("Keycloak:Enabled", "false")
        .WithEnvironment("RabbitMQ:Enabled", "false")
        .WithEnvironment("HealthChecks:Timeout", "30");

    Console.WriteLine("[AppHost] ✅ Configuração de teste concluída");
}
else if (builder.Environment.EnvironmentName == "Development")
{
    // Development environment - full-featured setup
    Console.WriteLine("[AppHost] Configurando ambiente de desenvolvimento...");
    
    var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
    {
        options.MainDatabase = "meajudaai";
        options.Username = "postgres";
        options.Password = "dev123";
        options.IncludePgAdmin = true;
    });

    var redis = builder.AddRedis("redis");

    var rabbitMq = builder.AddRabbitMQ("rabbitmq");

    var keycloak = builder.AddMeAjudaAiKeycloak(options =>
    {
        options.AdminUsername = "admin";
        options.AdminPassword = "admin123";
        options.DatabaseHost = "postgres-local";
        options.DatabasePort = "5432";
        options.DatabaseName = "meajudaai";
        options.DatabaseSchema = "identity";
        options.DatabaseUsername = "postgres";
        options.DatabasePassword = "dev123";
        options.ExposeHttpEndpoint = true;
    });

    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference((IResourceBuilder<IResourceWithConnectionString>)postgresql.MainDatabase, "DefaultConnection")
        .WithReference(redis)
        .WaitFor(redis)
        .WithReference(rabbitMq)
        .WaitFor(rabbitMq)
        .WithReference(keycloak.Keycloak)
        .WaitFor(keycloak.Keycloak)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

    Console.WriteLine("[AppHost] ✅ Configuração de desenvolvimento concluída");
}
else
{
    // Production environment - Azure resources
    Console.WriteLine("[AppHost] Configurando ambiente de produção...");
    
    var postgresql = builder.AddMeAjudaAiAzurePostgreSQL(options =>
    {
        options.MainDatabase = "meajudaai";
        options.Username = "postgres";
    });

    var redis = builder.AddRedis("redis");

    var serviceBus = builder.AddAzureServiceBus("servicebus");

    var keycloak = builder.AddMeAjudaAiKeycloakProduction(options =>
    {
        options.AdminUsername = "admin";
        options.DatabaseUsername = "postgres";
        options.ExposeHttpEndpoint = true;
    });

    builder.AddAzureContainerAppEnvironment("cae");

    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference((IResourceBuilder<IResourceWithConnectionString>)postgresql.MainDatabase, "DefaultConnection")
        .WithReference(redis)
        .WaitFor(redis)
        .WithReference(serviceBus)
        .WaitFor(serviceBus)
        .WithReference(keycloak.Keycloak)
        .WaitFor(keycloak.Keycloak)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName);

    Console.WriteLine("[AppHost] ✅ Configuração de produção concluída");
}

builder.Build().Run();