using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Idempotency;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration.ContextQueries;

/// <summary>
/// Base para testes de integração do repositório de idempotência do Ratings.
/// Usa PostgreSQL real via Testcontainers pois o repositório usa raw SQL (ON CONFLICT).
/// </summary>
public abstract class RatingsIdempotencyTestBase : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"ratings_idempotency_test_{GetType().Name}",
                Username = "test_user",
                Password = "test_password",
                Schema = "ratings"
            },
            Cache = new TestCacheOptions { Enabled = false },
            ExternalServices = new TestExternalServicesOptions
            {
                UseMessageBusMock = true
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddRatingsIdempotencyTestInfrastructure(options);
    }
}

public static class RatingsIdempotencyTestExtensions
{
    public static IServiceCollection AddRatingsIdempotencyTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);

        services.AddLocalization();
        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        services.AddDbContext<RatingsDbContext>((sp, dbOptions) =>
        {
            var connStr = SharedTestContainers.PostgreSql.GetConnectionString();
            dbOptions.UseNpgsql(connStr, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(RatingsDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ratings");
            })
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning))
            .EnableSensitiveDataLogging(false);
        });

        services.AddScoped<IIdempotencyRepository>(sp =>
            new RatingsIdempotencyRepository(sp.GetRequiredService<RatingsDbContext>()));

        return services;
    }
}
