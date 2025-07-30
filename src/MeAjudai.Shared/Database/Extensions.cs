using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MeAjudaAi.Shared.Database;

public static class Extensions
{
    private const string SectionName = "Postgres";

    public static IServiceCollection AddPostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgresOptions>(configuration.GetSection(SectionName));
        services.AddHostedService<DbContextInitializer>();

        // Fix para EF Core timestamp behavior
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return services;
    }

    public static IServiceCollection AddPostgresContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? connectionStringKey = null)
        where TContext : DbContext
    {
        var connectionString = GetConnectionString(configuration, connectionStringKey);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connectionString, ConfigureNpgsql);
            ConfigureDbContext(options);
        });

        return services;
    }

    public static IServiceCollection AddPostgresContext<TContext>(
        this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            var postgresOptions = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;

            options.UseNpgsql(postgresOptions.ConnectionString, ConfigureNpgsql);
            ConfigureDbContext(options);
        });

        return services;
    }

    public static IServiceCollection AddDapper(this IServiceCollection services)
    {
        services.AddScoped<IDapperConnection, DapperConnection>();
        return services;
    }

    private static string GetConnectionString(IConfiguration configuration, string? key)
    {
        if (key != null)
        {
            return configuration.GetConnectionString(key)
                ?? throw new InvalidOperationException($"Connection string '{key}' not found");
        }

        var postgresOptions = new PostgresOptions();
        configuration.GetSection(SectionName).Bind(postgresOptions);

        if (!string.IsNullOrEmpty(postgresOptions.ConnectionString))
        {
            return postgresOptions.ConnectionString;
        }

        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No connection string found");
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