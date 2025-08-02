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
            .Configure(opts => configuration.GetSection(PostgresOptions.SectionName).Bind(opts))
            .Validate(opts => !string.IsNullOrEmpty(opts.ConnectionString),
                "PostgreSQL connection string not found. Configure 'Postgres:ConnectionString' in appsettings.json")
            .ValidateOnStart();

        services.AddHostedService<DbContextInitializer>();

        // Fix para EF Core timestamp behavior
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
}