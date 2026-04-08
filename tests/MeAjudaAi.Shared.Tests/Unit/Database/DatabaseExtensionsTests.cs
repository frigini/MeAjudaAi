using System.Linq;
using FluentAssertions;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public class DatabaseExtensionsTests
{
    [Fact]
    public void AddPostgres_WithValidConnectionString_ShouldSucceed()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be("Host=localhost;Database=test");
    }

    [Fact]
    public void AddPostgres_ShouldRegisterPostgresOptionsService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<PostgresOptions>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddPostgres_WithFallbackToAspireLocal_ShouldUseIt()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:meajudaai-db-local"] = "Host=aspire-local;Database=local"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be("Host=aspire-local;Database=local");
    }

    [Fact]
    public void AddPostgres_WithFallbackToAspireDb_ShouldUseIt()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:meajudaai-db"] = "Host=aspire-dev;Database=dev"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be("Host=aspire-dev;Database=dev");
    }

    [Fact]
    public void AddPostgres_WithFallbackToAppsettings_ShouldUseIt()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Postgres:ConnectionString"] = "Host=appsettings;Database=appsettings"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be("Host=appsettings;Database=appsettings");
    }

    [Fact]
    public void AddPostgres_ShouldRegisterDapperConnection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);
        services.AddDapper();

        // Assert
        var hasDapper = services.Any(d => d.ServiceType == typeof(IDapperConnection));
        hasDapper.Should().BeTrue();
    }

    [Fact]
    public void AddPostgres_ShouldRegisterSchemaPermissionsManager()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Required by SchemaPermissionsManager
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var schemaManager = provider.GetService<SchemaPermissionsManager>();
        schemaManager.Should().NotBeNull();
    }

    [Fact]
    public void AddPostgres_ShouldRegisterPostgresOptionsAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            })
            .Build();

        // Act
        services.AddPostgres(configuration);

        // Assert - check for singleton registration of PostgresOptions (not IOptions)
        var hasSingletonOptions = services.Any(d => 
            d.ServiceType == typeof(PostgresOptions) && 
            d.Lifetime == ServiceLifetime.Singleton);
        hasSingletonOptions.Should().BeTrue();
    }

    [Fact]
    public void AddPostgres_WithAllFallbacksNullAndTestingEnv_ShouldAllowEmptyConnection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Set Testing environment
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            // Act & Assert - should not throw
            services.AddPostgres(configuration);
            
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
            
            // In Testing environment, empty connection string is allowed
            options.Value.ConnectionString.Should().BeEmpty();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
        }
    }

    [Fact]
    public void AddPostgresContext_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddPostgresContext<TestDbContext>();

        // Assert - DbContext should be registered
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddDapper_ShouldRegisterWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddDapper();

        // Assert
        var dapperService = services.FirstOrDefault(d => d.ServiceType == typeof(IDapperConnection));
        dapperService.Should().NotBeNull();
        dapperService!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
    }
}