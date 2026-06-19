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
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

        services.AddPostgres(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be(DatabaseConstants.LocalConnectionString);
    }

    [Fact]
    public void AddPostgres_ShouldRegisterPostgresOptionsService()
    {
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

        services.AddPostgres(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<PostgresOptions>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddPostgres_WithFallbackToAspireLocal_ShouldUseIt()
    {
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:meajudaai-db-local", DatabaseConstants.AspireLocalConnectionString));

        services.AddPostgres(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be(DatabaseConstants.AspireLocalConnectionString);
    }

    [Fact]
    public void AddPostgres_WithFallbackToAspireDb_ShouldUseIt()
    {
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:meajudaai-db", DatabaseConstants.AspireDevConnectionString));

        services.AddPostgres(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be(DatabaseConstants.AspireDevConnectionString);
    }

    [Fact]
    public void AddPostgres_WithFallbackToAppsettings_ShouldUseIt()
    {
        var (services, configuration) = CreateConfig(
            ("Postgres:ConnectionString", "Host=appsettings;Database=appsettings"));

        services.AddPostgres(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be("Host=appsettings;Database=appsettings");
    }

    [Fact]
    public void AddPostgres_ShouldRegisterDapperConnection()
    {
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

        services.AddPostgres(configuration);
        services.AddDapper();

        var hasDapper = services.Any(d => d.ServiceType == typeof(IDapperConnection));
        hasDapper.Should().BeTrue();
    }

    [Fact]
    public void AddPostgres_ShouldRegisterSchemaPermissionsManager()
    {
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));
        services.AddLogging();

        services.AddPostgres(configuration);

        var provider = services.BuildServiceProvider();
        var schemaManager = provider.GetService<SchemaPermissionsManager>();
        schemaManager.Should().NotBeNull();
    }

    [Fact]
    public void AddPostgres_ShouldRegisterPostgresOptionsAsSingleton()
    {
        var (services, configuration) = CreateConfig(
            ("ConnectionStrings:DefaultConnection", DatabaseConstants.LocalConnectionString));

        services.AddPostgres(configuration);

        var hasSingletonOptions = services.Any(d => 
            d.ServiceType == typeof(PostgresOptions) && 
            d.Lifetime == ServiceLifetime.Singleton);
        hasSingletonOptions.Should().BeTrue();
    }

    [Fact]
    public void AddPostgres_WithAllFallbacksNullAndTestingEnv_ShouldAllowEmptyConnection()
    {
        var (services, configuration) = CreateConfig(
            ("Postgres:TestConnectionString", DatabaseConstants.DummyConnectionString));
        var environment = new MockHostEnvironment("Testing");

        services.AddPostgres(configuration, environment);
        
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>();
        
        options.Value.ConnectionString.Should().Be(DatabaseConstants.DummyConnectionString);
    }

    [Fact]
    public void AddDapper_ShouldRegisterWithScopedLifetime()
    {
        var services = new ServiceCollection();
        
        services.AddDapper();

        var dapperService = services.FirstOrDefault(d => d.ServiceType == typeof(IDapperConnection));
        dapperService.Should().NotBeNull();
        dapperService!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ConfigureSchemaIsolation_WhenDisabled_ShouldNotRegisterHostedService()
    {
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "false"));

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);

        var hasHostedService = services.Any(d => d.ServiceType == typeof(IHostedService));
        hasHostedService.Should().BeFalse();
    }

    [Fact]
    public void ConfigureSchemaIsolation_WhenEnabledButMissingPasswords_ShouldThrow()
    {
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"));

        var act = () => services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RolePassword*");
    }

    [Fact]
    public void ConfigureSchemaIsolation_WhenEnabledWithPasswords_ShouldRegisterHostedService()
    {
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"),
            ("Postgres:SchemaIsolation:RolePassword", "pass1"),
            ("Postgres:SchemaIsolation:AppRolePassword", "pass2"));
        services.AddLogging();
        services.AddSingleton<SchemaPermissionsManager>();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);

        var hasHostedService = services.Any(d => d.ServiceType == typeof(IHostedService));
        hasHostedService.Should().BeTrue();
    }

    [Fact]
    public async Task SchemaIsolationService_StartAsync_WhenNoConnectionString_ShouldReturnWithoutExecuting()
    {
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

        await hostedService.StartAsync(CancellationToken.None);

        mockManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SchemaIsolationService_StartAsync_WhenPermissionsAlreadyConfigured_ShouldSkip()
    {
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

        await hostedService.StartAsync(CancellationToken.None);

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

        await hostedService.StartAsync(CancellationToken.None);

        mockManager.Verify(
            m => m.EnsureModulePermissionsAsync(It.IsAny<string>(), It.IsAny<ModulePermissionConfig>()),
            Times.Once);
    }

    [Fact]
    public async Task SchemaIsolationService_StopAsync_ShouldCompleteImmediately()
    {
        var (services, configuration) = CreateConfig(
            ("Postgres:SchemaIsolation:Enabled", "true"),
            ("Postgres:SchemaIsolation:RolePassword", "p1"),
            ("Postgres:SchemaIsolation:AppRolePassword", "p2"));
        services.AddLogging();
        services.AddSingleton(new Mock<SchemaPermissionsManager>(
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SchemaPermissionsManager>>()).Object);

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Users, Schemas.Users, DatabaseRoleConstants.Users);
        var hostedService = services.BuildServiceProvider().GetRequiredService<IHostedService>();

        await hostedService.StopAsync(CancellationToken.None);
    }
}