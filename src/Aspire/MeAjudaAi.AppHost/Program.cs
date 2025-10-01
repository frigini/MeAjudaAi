using MeAjudaAi.AppHost.Extensions;
using MeAjudaAi.AppHost.Helpers;

var builder = DistributedApplication.CreateBuilder(args);

// Detecção robusta de ambiente de teste
var isTestingEnv = EnvironmentHelpers.IsTesting(builder);

if (isTestingEnv)
{
    // Ambiente de teste - configuração simplificada para testes mais rápidos
    // Lê credenciais do banco de dados de variáveis de ambiente para maior segurança
    var testDbName = Environment.GetEnvironmentVariable("MEAJUDAAI_DB") ?? "meajudaai";
    var testDbUser = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_USER") ?? "postgres";
    var testDbPassword = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PASS") ?? string.Empty;
    
    // Em ambiente de CI, a senha deve ser fornecida via variável de ambiente
    if (string.IsNullOrEmpty(testDbPassword) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
    {
        throw new InvalidOperationException("MEAJUDAAI_DB_PASS environment variable is required in CI environment");
    }
    
    var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
    {
        options.IsTestEnvironment = true;
        options.MainDatabase = testDbName;
        options.Username = testDbUser;
        options.Password = testDbPassword;
    });

    var redis = builder.AddRedis("redis");

    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference((IResourceBuilder<IResourceWithConnectionString>)postgresql.MainDatabase, "DefaultConnection")
        .WithReference(redis)
        .WaitFor(postgresql.MainDatabase)
        .WaitFor(redis)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Testing")
        .WithEnvironment("Logging:LogLevel:Default", "Information")
        .WithEnvironment("Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning")
        .WithEnvironment("Logging:LogLevel:Microsoft.Hosting.Lifetime", "Information")
        .WithEnvironment("Keycloak:Enabled", "false")
        .WithEnvironment("RabbitMQ:Enabled", "false")
        .WithEnvironment("HealthChecks:Timeout", "30");
}
else if (EnvironmentHelpers.IsDevelopment(builder))
{
    // Ambiente de desenvolvimento - configuração completa
    // Lê credenciais de variáveis de ambiente com fallbacks seguros para desenvolvimento
    var mainDatabase = Environment.GetEnvironmentVariable("MAIN_DATABASE") ?? "meajudaai";
    var dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres";
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "dev123";
    var includePgAdminStr = Environment.GetEnvironmentVariable("INCLUDE_PGADMIN") ?? "true";
    var includePgAdmin = bool.TryParse(includePgAdminStr, out var pgAdminResult) ? pgAdminResult : true;
    
    var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
    {
        options.MainDatabase = mainDatabase;
        options.Username = dbUsername;
        options.Password = dbPassword;
        options.IncludePgAdmin = includePgAdmin;
    });

    var redis = builder.AddRedis("redis");

    var rabbitMq = builder.AddRabbitMQ("rabbitmq");

    var keycloak = builder.AddMeAjudaAiKeycloak(options =>
    {
        // Lê configuração do Keycloak de variáveis de ambiente ou configuração
        options.AdminUsername = builder.Configuration["Keycloak:AdminUsername"] 
                               ?? Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_USERNAME") 
                               ?? "admin";
        options.AdminPassword = builder.Configuration["Keycloak:AdminPassword"] 
                               ?? Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD") 
                               ?? "admin123";
        options.DatabaseHost = builder.Configuration["Keycloak:DatabaseHost"] 
                              ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_HOST") 
                              ?? "postgres-local";
        options.DatabasePort = builder.Configuration["Keycloak:DatabasePort"] 
                              ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_PORT") 
                              ?? "5432";
        options.DatabaseName = builder.Configuration["Keycloak:DatabaseName"] 
                              ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_NAME") 
                              ?? mainDatabase;
        options.DatabaseSchema = builder.Configuration["Keycloak:DatabaseSchema"] 
                                 ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_SCHEMA") 
                                 ?? "identity";
        options.DatabaseUsername = builder.Configuration["Keycloak:DatabaseUsername"] 
                                   ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_USERNAME") 
                                   ?? dbUsername;
        options.DatabasePassword = builder.Configuration["Keycloak:DatabasePassword"] 
                                   ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_PASSWORD") 
                                   ?? dbPassword;
        
        var exposeHttpStr = builder.Configuration["Keycloak:ExposeHttpEndpoint"] 
                           ?? Environment.GetEnvironmentVariable("KEYCLOAK_EXPOSE_HTTP");
        options.ExposeHttpEndpoint = bool.TryParse(exposeHttpStr, out var exposeResult) ? exposeResult : true;
    });

    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference((IResourceBuilder<IResourceWithConnectionString>)postgresql.MainDatabase, "DefaultConnection")
        .WithReference(redis)
        .WaitFor(postgresql.MainDatabase)
        .WaitFor(redis)
        .WithReference(rabbitMq)
        .WaitFor(rabbitMq)
        .WithReference(keycloak.Keycloak)
        .WaitFor(keycloak.Keycloak)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", EnvironmentHelpers.GetEnvironmentName(builder));
}
else if (EnvironmentHelpers.IsProduction(builder))
{
    // Ambiente de produção - recursos Azure
    var postgresql = builder.AddMeAjudaAiAzurePostgreSQL(options =>
    {
        options.MainDatabase = "meajudaai";
        options.Username = "postgres";
    });

    var redis = builder.AddRedis("redis");

    var serviceBus = builder.AddAzureServiceBus("servicebus");

    var keycloak = builder.AddMeAjudaAiKeycloakProduction();

    builder.AddAzureContainerAppEnvironment("cae");

    var apiService = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
        .WithReference((IResourceBuilder<IResourceWithConnectionString>)postgresql.MainDatabase, "DefaultConnection")
        .WithReference(redis)
        .WaitFor(postgresql.MainDatabase)
        .WaitFor(redis)
        .WithReference(serviceBus)
        .WaitFor(serviceBus)
        .WithReference(keycloak.Keycloak)
        .WaitFor(keycloak.Keycloak)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", EnvironmentHelpers.GetEnvironmentName(builder));
}
else
{
    // Fail-closed: ambiente não suportado
    var currentEnv = EnvironmentHelpers.GetEnvironmentName(builder);
    var errorMessage = $"Unsupported environment: '{currentEnv}'. Only Testing, Development, and Production environments are supported.";
    
    Console.Error.WriteLine($"ERROR: {errorMessage}");
    Environment.Exit(1);
}

builder.Build().Run();