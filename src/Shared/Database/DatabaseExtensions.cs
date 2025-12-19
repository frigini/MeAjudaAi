using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>()
            .Configure(opts =>
            {
                // Tenta múltiplas fontes de string de conexão em ordem de preferência
                opts.ConnectionString =
                    configuration.GetConnectionString("DefaultConnection") ??  // Sobrescrita para testes
                    configuration.GetConnectionString("meajudaai-db-local") ??  // Aspire para testes
                    configuration.GetConnectionString("meajudaai-db") ??        // Aspire para desenvolvimento
                    configuration["Postgres:ConnectionString"] ??              // Configuração manual
                    string.Empty;
            });

        // Só valida a connection string em ambientes que não sejam Testing
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                         Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");

        var isTestingEnvironment = environment == "Testing" ||
                                 environment?.Equals("Testing", StringComparison.OrdinalIgnoreCase) == true ||
                                 integrationTests == "true" ||
                                 integrationTests == "1";

        if (!isTestingEnvironment)
        {
            services.Configure<PostgresOptions>(opts =>
            {
                if (string.IsNullOrEmpty(opts.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "PostgreSQL connection string not found. Configure connection string via Aspire, 'Postgres:ConnectionString' in appsettings.json, or as ConnectionStrings:meajudaai-db");
                }
            });
        }

        // Monitoramento essencial de banco de dados
        services.AddDatabaseMonitoring();

        // Gerenciador de permissões de schema para isolamento entre módulos
        services.AddSingleton<SchemaPermissionsManager>();

        // Correção para comportamento de timestamp do EF Core
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return services;
    }

    /// <summary>
    /// Adiciona DbContext configurado com PostgreSQL
    /// </summary>
    public static IServiceCollection AddPostgresContext<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(ConfigureWithPostgresOptions);
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
    /// Configura permissões de schema para o módulo Users usando scripts existentes.
    /// Use em produção para segurança do módulo.
    /// </summary>
    public static async Task<IServiceCollection> EnsureUsersSchemaPermissionsAsync(
        this IServiceCollection services,
        IConfiguration configuration,
        string? usersRolePassword = null,
        string? appRolePassword = null)
    {
        // Obter string de conexão admin
        var adminConnectionString =
            configuration.GetConnectionString("meajudaai-db-admin") ??
            configuration.GetConnectionString("meajudaai-db") ??
            configuration["Postgres:ConnectionString"];

        if (string.IsNullOrEmpty(adminConnectionString))
        {
            throw new InvalidOperationException("Admin connection string not found for schema permissions setup");
        }

        // Usar senhas da configuração ou padrões para desenvolvimento
        usersRolePassword ??= configuration["Postgres:UsersRolePassword"] ?? "users_secret";
        appRolePassword ??= configuration["Postgres:AppRolePassword"] ?? "app_secret";

        // Configurar permissões se necessário
        using var serviceProvider = services.BuildServiceProvider();
        var permissionsManager = serviceProvider.GetRequiredService<SchemaPermissionsManager>();

        if (!await permissionsManager.AreUsersPermissionsConfiguredAsync(adminConnectionString))
        {
            await permissionsManager.EnsureUsersModulePermissionsAsync(adminConnectionString, usersRolePassword, appRolePassword);
        }

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
