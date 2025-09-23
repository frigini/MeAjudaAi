using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MeAjudaAi.Shared.Database;

public static class Extensions
{
    public static IServiceCollection AddPostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>()
            .Configure(opts => 
            {
                // Try multiple connection string sources in order of preference
                opts.ConnectionString = 
                    configuration.GetConnectionString("meajudaai-db-local") ??  // Aspire testing
                    configuration.GetConnectionString("meajudaai-db") ??        // Aspire development
                    configuration["Postgres:ConnectionString"] ??              // Manual configuration
                    string.Empty;
            })
            .Validate(opts => !string.IsNullOrEmpty(opts.ConnectionString),
                "PostgreSQL connection string not found. Configure connection string via Aspire, 'Postgres:ConnectionString' in appsettings.json, or as ConnectionStrings:meajudaai-db")
            .ValidateOnStart();

        // Database monitoring essencial
        services.AddDatabaseMonitoring();

        // Schema permissions manager para isolamento entre módulos
        services.AddSingleton<SchemaPermissionsManager>();

        // Fix para EF Core timestamp behavior
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
        // Obter connection string admin
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

    public static IServiceCollection AddPostgresContext<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(ConfigureWithPostgresOptions);
        return services;
    }

    public static IServiceCollection AddDapper(this IServiceCollection services)
    {
        services.AddScoped<IDapperConnection, DapperConnection>();
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

    /// <summary>
    /// Adiciona monitoring essencial de database
    /// </summary>
    public static IServiceCollection AddDatabaseMonitoring(this IServiceCollection services)
    {
        // Registra métricas de database
        services.AddSingleton<DatabaseMetrics>();
        
        // Registra interceptor para Entity Framework
        services.AddSingleton<DatabaseMetricsInterceptor>();
        
        return services;
    }
}