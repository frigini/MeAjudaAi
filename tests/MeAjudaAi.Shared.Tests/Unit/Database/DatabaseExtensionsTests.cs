using MeAjudaAi.Shared.Authorization.Core.Models;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public class DatabaseExtensionsTests
{
    private static (ServiceCollection Services, IConfiguration Configuration) CreateConfig(
        params (string Key, string? Value)[] settings)
    {
        var services = new ServiceCollection();
        var dict = settings.ToDictionary(x => x.Key, x => x.Value);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
        return (services, configuration);
    }

    [Fact]
    public void AddPostgres_WithValidConnectionString_ShouldSucceed()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        options.Value.ConnectionString.Should().Be(DatabaseConstants.LocalConnectionString);
    }

    [Fact]
    public void AddPostgres_ShouldRegisterPostgresOptionsService()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

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
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:meajudaai-db-local", DatabaseConstants.AspireLocalConnectionString));

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        options.Value.ConnectionString.Should().Be(DatabaseConstants.AspireLocalConnectionString);
    }

    [Fact]
    public void AddPostgres_WithFallbackToAspireDb_ShouldUseIt()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:meajudaai-db", DatabaseConstants.AspireDevConnectionString));

        // Act
        services.AddPostgres(configuration);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        options.Value.ConnectionString.Should().Be(DatabaseConstants.AspireDevConnectionString);
    }

    [Fact]
    public void AddPostgres_WithFallbackToAppsettings_ShouldUseIt()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:ConnectionString", "Host=appsettings;Database=appsettings"));

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
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

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
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));
        services.AddLogging();

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
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

        // Act
        services.AddPostgres(configuration);

        // Assert
        var hasSingletonOptions = services.Any(d =>
            d.ServiceType == typeof(PostgresOptions) &&
            d.Lifetime == ServiceLifetime.Singleton);
        hasSingletonOptions.Should().BeTrue();
    }

    [Fact]
    public void AddPostgres_WithAllFallbacksNullAndTestingEnv_ShouldAllowEmptyConnection()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:TestConnectionString", DatabaseConstants.DummyConnectionString));
        var environment = new MockHostEnvironment("Testing");

        // Act
        services.AddPostgres(configuration, environment);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        options.Value.ConnectionString.Should().Be(DatabaseConstants.DummyConnectionString);
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

    [Fact]
    public void ConfigureSchemaIsolation_WhenDisabled_ShouldNotRegisterHostedService()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "false"));

        // Act
        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);

        // Assert
        var hasHostedService = services.Any(d => d.ServiceType == typeof(IHostedService));
        hasHostedService.Should().BeFalse();
    }

    [Fact]
    public void ConfigureSchemaIsolation_WhenEnabledButMissingPasswords_ShouldThrow()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"));

        // Act
        var act = () => services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RolePassword*");
    }

    [Fact]
    public void ConfigureSchemaIsolation_WhenEnabledWithPasswords_ShouldRegisterHostedService()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"),
            ("Postgres:SchemaIsolation:RolePassword", "pass1"),
            ("Postgres:SchemaIsolation:AppRolePassword", "pass2"));
        services.AddLogging();
        services.AddSingleton<SchemaPermissionsManager>();

        // Act
        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);

        // Assert
        var hasHostedService = services.Any(d => d.ServiceType == typeof(IHostedService));
        hasHostedService.Should().BeTrue();
    }

    [Fact]
    public async Task SchemaIsolationService_StartAsync_WhenNoConnectionString_ShouldReturnWithoutExecuting()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"),
            ("Postgres:SchemaIsolation:RolePassword", "pass1"),
            ("Postgres:SchemaIsolation:AppRolePassword", "pass2"));
        services.AddLogging();
        var mockManager = new Mock<SchemaPermissionsManager>(MockBehavior.Strict,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SchemaPermissionsManager>>());
        services.AddSingleton(mockManager.Object);
        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);
        var provider = services.BuildServiceProvider();
        var hostedService = provider.GetRequiredService<IHostedService>();

        // Act
        await hostedService.StartAsync(CancellationToken.None);

        // Assert
        mockManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SchemaIsolationService_StartAsync_WhenPermissionsAlreadyConfigured_ShouldSkip()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"),
            ("Postgres:SchemaIsolation:RolePassword", "pass1"),
            ("Postgres:SchemaIsolation:AppRolePassword", "pass2"),
            ("ConnectionStrings:meajudaai-db", DatabaseConstants.LocalConnectionString));
        services.AddLogging();
        var mockManager = new Mock<SchemaPermissionsManager>(
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SchemaPermissionsManager>>());
        mockManager
            .Setup(m => m.AreModulePermissionsConfiguredAsync(It.IsAny<string>(), Schemas.Users, DatabaseRoleConstants.Users))
            .ReturnsAsync(true);
        services.AddSingleton(mockManager.Object);
        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);
        var provider = services.BuildServiceProvider();
        var hostedService = provider.GetRequiredService<IHostedService>();

        // Act
        await hostedService.StartAsync(CancellationToken.None);

        // Assert
        mockManager.Verify(
            m => m.AreModulePermissionsConfiguredAsync(It.IsAny<string>(), Schemas.Users, DatabaseRoleConstants.Users),
            Times.Once);
        mockManager.Verify(
            m => m.EnsureModulePermissionsAsync(It.IsAny<string>(), It.IsAny<ModulePermissionConfig>()),
            Times.Never);
    }

    [Fact]
    public async Task SchemaIsolationService_StartAsync_WhenPermissionsNotConfigured_ShouldEnsurePermissions()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"),
            ("Postgres:SchemaIsolation:RolePassword", "pass1"),
            ("Postgres:SchemaIsolation:AppRolePassword", "pass2"),
            ("ConnectionStrings:meajudaai-db", DatabaseConstants.LocalConnectionString));
        services.AddLogging();
        var mockManager = new Mock<SchemaPermissionsManager>(
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SchemaPermissionsManager>>());
        mockManager
            .Setup(m => m.AreModulePermissionsConfiguredAsync(It.IsAny<string>(), Schemas.Users, DatabaseRoleConstants.Users))
            .ReturnsAsync(false);
        mockManager
            .Setup(m => m.EnsureModulePermissionsAsync(It.IsAny<string>(), It.IsAny<ModulePermissionConfig>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(mockManager.Object);
        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);
        var provider = services.BuildServiceProvider();
        var hostedService = provider.GetRequiredService<IHostedService>();

        // Act
        await hostedService.StartAsync(CancellationToken.None);

        // Assert
        mockManager.Verify(
            m => m.EnsureModulePermissionsAsync(It.IsAny<string>(), It.IsAny<ModulePermissionConfig>()),
            Times.Once);
    }

    [Fact]
    public async Task SchemaIsolationService_StopAsync_ShouldCompleteImmediately()
    {
        // Arrange
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"),
            ("Postgres:SchemaIsolation:RolePassword", "p1"),
            ("Postgres:SchemaIsolation:AppRolePassword", "p2"));
        services.AddLogging();
        services.AddSingleton(new Mock<SchemaPermissionsManager>(
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SchemaPermissionsManager>>()).Object);
        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);
        var hostedService = services.BuildServiceProvider().GetRequiredService<IHostedService>();

        // Act
        await hostedService.StopAsync(CancellationToken.None);
    }
}
