using MeAjudaAi.Shared.Authorization.Core.Models;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Extension methods para configuração de Database e PostgreSQL
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adiciona PostgreSQL com configuração completa
    /// </summary>
    public static IServiceCollection AddPostgres(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        services.AddOptions<PostgresOptions>()
            .Configure(opts =>
            {
                // Tenta múltiplas fontes de string de conexão em ordem de preferência
                var conn = configuration.GetConnectionString("DefaultConnection") ?? 
                           configuration.GetConnectionString("meajudaai-db-local") ??
                           configuration.GetConnectionString("meajudaai-db") ??
                           configuration["Postgres:ConnectionString"];

                if (string.IsNullOrEmpty(conn))
                {
                    var envName = environment?.EnvironmentName ?? 
                                  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                                  Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                    
                    if (envName == EnvironmentNames.Testing)
                    {
                        // Leia uma string de conexão de teste do arquivo de configuração ou de ambiente em vez de codificar as credenciais diretamente no código
                        conn = configuration["Postgres:TestConnectionString"] ??
                               Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTIONSTRING");

                        if (string.IsNullOrEmpty(conn))
                        {
                            throw new InvalidOperationException(
                                "Test environment detected: set 'Postgres:TestConnectionString' (appsettings) or 'POSTGRES_TEST_CONNECTIONSTRING' (env) with the test DB connection string.");
                        }
                    }
                }
                opts.ConnectionString = conn ?? string.Empty;
                Console.WriteLine($"[DEBUG] Postgres ConnectionString resolved: {opts.ConnectionString}");
            });

        // Só valida a connection string em ambientes que não sejam Testing
        var environmentName = environment?.EnvironmentName ?? 
                               Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                               Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");

        var isTestingEnvironment = environmentName == EnvironmentNames.Testing ||
                                 environmentName?.Equals("Testing", StringComparison.OrdinalIgnoreCase) == true ||
                                 integrationTests == "true" ||
                                 integrationTests == "1";

        if (!isTestingEnvironment)
        {
            services.PostConfigure<PostgresOptions>(opts =>
            {
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "PostgreSQL connection string not found. Configure connection string via Aspire, 'Postgres:ConnectionString' in appsettings.json, or as ConnectionStrings:meajudaai-db");
                }
            });
        }

        // Registra PostgresOptions como singleton para injeção direta (ex: DapperConnection)
        services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PostgresOptions>>().Value);

        // Monitoramento essencial de banco de dados
        services.AddDatabaseMonitoring();

        // Gerenciador de permissões de schema para isolamento entre módulos
        services.AddSingleton<SchemaPermissionsManager>();

        // Correção para comportamento de timestamp do EF Core
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return services;
    }

    /// <summary>
    /// Adiciona DbContext configurado com PostgreSQL e permite configuração adicional.
    /// </summary>
    public static IServiceCollection AddPostgresContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            ConfigureWithPostgresOptions(serviceProvider, options);
            optionsAction?.Invoke(options);
        });
        return services;
    }

    /// <summary>
    /// Adiciona suporte a Dapper para queries de alta performance
    /// </summary>
    public static IServiceCollection AddDapper(this IServiceCollection services)
    {
        services.AddScoped<IDapperConnection, DapperConnection>();
        return services;
    }

    /// <summary>
    /// Configura schema isolation para TODOS os módulos de uma vez.
    /// Use este método no Program.cs da aplicação principal para garantir que todos os módulos
    /// tenham suas permissões configuradas automaticamente.
    /// </summary>
    public static IServiceCollection ConfigureAllModulesSchemaIsolation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var modules = new[]
        {
            (ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users),
            (ModuleNames.Providers, Schemas.Providers, DatabaseRoleConstants.Providers),
            (ModuleNames.Documents, Schemas.Documents, DatabaseRoleConstants.Documents),
            (ModuleNames.ServiceCatalogs, Schemas.ServiceCatalogs, DatabaseRoleConstants.ServiceCatalogs),
            (ModuleNames.SearchProviders, Schemas.SearchProviders, DatabaseRoleConstants.SearchProviders),
            (ModuleNames.Locations, Schemas.Locations, DatabaseRoleConstants.Locations),
            (ModuleNames.Bookings, Schemas.Bookings, DatabaseRoleConstants.Bookings),
            (ModuleNames.Communications, Schemas.Communications, DatabaseRoleConstants.Communications),
            (ModuleNames.Payments, Schemas.Payments, DatabaseRoleConstants.Payments),
            (ModuleNames.Ratings, Schemas.Ratings, DatabaseRoleConstants.Ratings)
        };

        foreach (var (moduleName, schemaName, roleName) in modules)
        {
            services.ConfigureSchemaIsolation(configuration, moduleName, schemaName, roleName);
        }

        return services;
    }

    /// <summary>
    /// Helper que configura schema isolation para um módulo se habilitado via configuração.
    /// Registra um IHostedService que executa after o container está pronto.
    /// </summary>
    public static IServiceCollection ConfigureSchemaIsolation(
        this IServiceCollection services,
        IConfiguration configuration,
        string moduleName,
        string schemaName,
        string roleName)
    {
        var enabled = configuration.GetValue("Postgres:SchemaIsolation:Enabled", false);
        if (!enabled)
            return services;

        var rolePassword = configuration["Postgres:SchemaIsolation:RolePassword"];
        var appRolePassword = configuration["Postgres:SchemaIsolation:AppRolePassword"];

        if (string.IsNullOrWhiteSpace(rolePassword) || string.IsNullOrWhiteSpace(appRolePassword))
            throw new InvalidOperationException(
                $"Schema isolation is enabled for module '{moduleName}' but " +
                "'Postgres:SchemaIsolation:RolePassword' and 'Postgres:SchemaIsolation:AppRolePassword' are required.");

        var config = new ModulePermissionConfig(
            ModuleName: moduleName,
            SchemaName: schemaName,
            RoleName: roleName,
            RolePassword: rolePassword,
            AppRoleName: "meajudaai_app_role",
            AppRolePassword: appRolePassword);

        services.AddSingleton<IHostedService>(sp => new SchemaIsolationService(sp.GetRequiredService<SchemaPermissionsManager>(), configuration, config));

        return services;
    }

    /// <summary>
    /// Adiciona monitoramento essencial de banco de dados
    /// </summary>
    public static IServiceCollection AddDatabaseMonitoring(this IServiceCollection services)
    {
        // Registra métricas de banco de dados
        services.AddSingleton<DatabaseMetrics>();

        // Registra interceptor para Entity Framework
        services.AddSingleton<DatabaseMetricsInterceptor>();

        return services;
    }

    private static void ConfigureWithPostgresOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        var postgresOptions = serviceProvider.GetRequiredService<PostgresOptions>();
        var connectionString = postgresOptions.ConnectionString;

        // Resolve e adiciona o interceptor de métricas do DI
        var metricsInterceptor = serviceProvider.GetRequiredService<DatabaseMetricsInterceptor>();
        options.AddInterceptors(metricsInterceptor);

        ConfigurePostgresContext(options, connectionString);
    }

    private static void ConfigurePostgresContext(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseNpgsql(connectionString, ConfigureNpgsql);
        ConfigureDbContext(options);
    }

    private static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsqlOptions)
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }

    private static void ConfigureDbContext(DbContextOptionsBuilder options)
    {
        options.EnableSensitiveDataLogging(false);
        options.EnableServiceProviderCaching();
    }
}

/// <summary>
/// Serviço que executa schema isolation após o container DI estar pronto.
/// </summary>
file sealed class SchemaIsolationService(
    SchemaPermissionsManager manager,
    IConfiguration configuration,
    ModulePermissionConfig config) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var adminConnectionString =
            configuration.GetConnectionString("meajudaai-db-admin") ??
            configuration.GetConnectionString("meajudaai-db") ??
            configuration["Postgres:ConnectionString"];

        if (string.IsNullOrEmpty(adminConnectionString))
            return;

        if (!await manager.AreModulePermissionsConfiguredAsync(adminConnectionString, config.SchemaName, config.RoleName))
        {
            await manager.EnsureModulePermissionsAsync(adminConnectionString, config);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
