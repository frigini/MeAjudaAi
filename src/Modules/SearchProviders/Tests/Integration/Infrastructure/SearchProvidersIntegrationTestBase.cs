using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Integration.Infrastructure;

/// <summary>
/// Classe base para testes de integração do módulo SearchProviders.
/// Usa Testcontainers PostgreSQL com extensão PostGIS via BaseIntegrationTest.
/// </summary>
public abstract class SearchProvidersIntegrationTestBase : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions()
    {
        var testClassName = GetType().Name;
        if (testClassName.Length > 50)
            testClassName = testClassName[..50];

        var options = new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"search_test_{testClassName}",
                Username = "test_user",
                Password = "test_password",
                Schema = Schemas.SearchProviders
            },
            Cache = new TestCacheOptions { Enabled = false },
            ExternalServices = new TestExternalServicesOptions
            {
                UseKeycloakMock = true,
                UseMessageBusMock = true
            }
        };

        var baseConnectionString = SharedTestContainers.PostgreSql.GetConnectionString();
        var baseBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString);

        Console.WriteLine($"[SearchProviders Test] Base connection string components: Host={baseBuilder.Host}, Port={baseBuilder.Port}, Username={baseBuilder.Username}, Password={(!string.IsNullOrEmpty(baseBuilder.Password) ? "***" : "EMPTY")}, Database={baseBuilder.Database}");

        // Preserve all credentials when setting the test-specific database
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = baseBuilder.Host,
            Port = baseBuilder.Port,
            Username = baseBuilder.Username,
            Password = baseBuilder.Password,
            Database = options.Database.DatabaseName,
            SslMode = baseBuilder.SslMode
        };

        var finalConnectionString = builder.ToString();
        Console.WriteLine($"[SearchProviders Test] Final connection string components: Host={builder.Host}, Port={builder.Port}, Username={builder.Username}, Password={(!string.IsNullOrEmpty(builder.Password) ? "***" : "EMPTY")}, Database={builder.Database}");

        options.Database.ConnectionString = finalConnectionString;

        return options;
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddSearchProvidersTestInfrastructure(options);
    }

    protected override async Task OnInitializeAsync()
    {
        var dbContext = GetService<SearchProvidersDbContext>();

        var connection = dbContext.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;

        try
        {
            if (!wasOpen) await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT PostGIS_Version()";
            var version = await command.ExecuteScalarAsync();
            if (version == null)
                throw new InvalidOperationException("PostGIS extension is not available in the test database");
        }
        finally
        {
            if (!wasOpen && connection.State == System.Data.ConnectionState.Open)
                await connection.CloseAsync();
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM search_providers.searchable_providers WHERE true;");
    }

    protected SearchableProvider CreateTestSearchableProvider(
        string name,
        double latitude,
        double longitude,
        ESubscriptionTier tier = ESubscriptionTier.Free,
        string? description = null,
        string? city = null,
        string? state = null)
    {
        var providerId = Guid.NewGuid();
        var location = new GeoPoint(latitude, longitude);

        return SearchableProvider.Create(
            providerId: providerId,
            name: name,
            slug: SlugHelper.GenerateWithSuffix(name, providerId.ToString("N")[..8]),
            location: location,
            subscriptionTier: tier,
            description: description,
            city: city,
            state: state);
    }

    protected SearchableProvider CreateTestSearchableProviderWithProviderId(
        Guid providerId,
        string name,
        double latitude,
        double longitude,
        ESubscriptionTier tier = ESubscriptionTier.Free,
        string? description = null,
        string? city = null,
        string? state = null)
    {
        var location = new GeoPoint(latitude, longitude);

        return SearchableProvider.Create(
            providerId: providerId,
            name: name,
            slug: SlugHelper.GenerateWithSuffix(name, providerId.ToString("N")[..8]),
            location: location,
            subscriptionTier: tier,
            description: description,
            city: city,
            state: state);
    }

    protected async Task<SearchableProvider> PersistSearchableProviderAsync(SearchableProvider provider)
    {
        var dbContext = GetService<SearchProvidersDbContext>();
        await dbContext.SearchableProviders.AddAsync(provider);
        await dbContext.SaveChangesAsync();
        return provider;
    }

    protected async Task CleanupDatabase()
    {
        var dbContext = GetService<SearchProvidersDbContext>();

        try
        {
            await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE search_providers.searchable_providers CASCADE;");
        }
        catch
        {
            await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM search_providers.searchable_providers;");
        }

        var remainingCount = await dbContext.SearchableProviders.CountAsync();
        if (remainingCount > 0)
            throw new InvalidOperationException($"Database cleanup failed: {remainingCount} providers remain");
    }
}
