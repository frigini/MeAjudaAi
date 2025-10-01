using MeAjudaAi.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

// Detecção de ambiente de teste
var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var builderEnv = builder.Environment.EnvironmentName;
var isTestingEnv = envName == "Testing" ||
                  builderEnv == "Testing" ||
                  Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "true";

if (isTestingEnv)
{
    // Ambiente de teste - configuração simplificada para testes mais rápidos
    var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
    {
        options.IsTestEnvironment = true;
        options.MainDatabase = "meajudaai";
        options.Username = "postgres";
        options.Password = "dev123";
    });

    var redis = builder.AddRedis("redis");

    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference((IResourceBuilder<IResourceWithConnectionString>)postgresql.MainDatabase, "DefaultConnection")
        .WithReference(redis)
        .WaitFor(redis)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Testing")
        .WithEnvironment("Logging:LogLevel:Default", "Information")
        .WithEnvironment("Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning")
        .WithEnvironment("Logging:LogLevel:Microsoft.Hosting.Lifetime", "Information")
        .WithEnvironment("Keycloak:Enabled", "false")
        .WithEnvironment("RabbitMQ:Enabled", "false")
        .WithEnvironment("HealthChecks:Timeout", "30");
}
else if (builderEnv == "Development")
{
    // Ambiente de desenvolvimento - configuração completa
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
}
else
{
    // Ambiente de produção - recursos Azure
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
}

builder.Build().Run();