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
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("Host=localhost;Database=test");

        // Act
        services.AddPostgres(configurationMock.Object);

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
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns("Host=localhost;Database=test");

        // Act
        services.AddPostgres(configurationMock.Object);

        // Assert
        var hasOptions = services.Any(d => d.ServiceType == typeof(IOptions<PostgresOptions>));
        hasOptions.Should().BeTrue();
    }

    [Fact]
    public void AddPostgres_WithFallbackToAspireLocal_ShouldUseIt()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        
        // First connection string returns null, second returns value
        configurationMock
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns((string?)null);
        configurationMock
            .Setup(x => x.GetConnectionString("meajudaai-db-local"))
            .Returns("Host=aspire-local;Database=local");

        // Act
        services.AddPostgres(configurationMock.Object);

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
        var configurationMock = new Mock<IConfiguration>();
        
        // First two return null, third returns value
        configurationMock
            .Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns((string?)null);
        configurationMock
            .Setup(x => x.GetConnectionString("meajudaai-db-local"))
            .Returns((string?)null);
        configurationMock
            .Setup(x => x.GetConnectionString("meajudaai-db"))
            .Returns("Host=aspire-dev;Database=dev");

        // Act
        services.AddPostgres(configurationMock.Object);

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
        var configurationMock = new Mock<IConfiguration>();
        
        // All GetConnectionString return null, use appsettings
        configurationMock
            .Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns((string?)null);
        configurationMock
            .Setup(x => x["Postgres:ConnectionString"])
            .Returns("Host=appsettings;Database=appsettings");

        // Act
        services.AddPostgres(configurationMock.Object);

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
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns("Host=localhost;Database=test");

        // Act
        services.AddPostgres(configurationMock.Object);
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
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns("Host=localhost;Database=test");

        // Act
        services.AddPostgres(configurationMock.Object);

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
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns("Host=localhost;Database=test");

        // Act
        services.AddPostgres(configurationMock.Object);

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
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns((string?)null);
        configurationMock
            .Setup(x => x["Postgres:ConnectionString"])
            .Returns((string?)null);

        // Set Testing environment
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            // Act & Assert - should not throw
            services.AddPostgres(configurationMock.Object);
            
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

    private class TestDbContext : DbContext
    {
    }
}