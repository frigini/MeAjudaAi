using MeAjudaAi.Modules.Users.API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API;

/// <summary>
/// Testes de integração dos métodos de extensão do módulo Users
/// Foca em cenários de integração e configuração completa
/// </summary>
[Trait("Category", "Integration")]
[Trait("Module", "Users")]
[Trait("Layer", "API")]
public class ExtensionsTests
{
    [Fact]
    public void AddUsersModule_ShouldAddApplicationAndInfrastructureServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=test;User Id=test;Password=test;",
                ["Keycloak:BaseUrl"] = "http://localhost:8080",
                ["Keycloak:Realm"] = "test-realm",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:ClientSecret"] = "test-secret"
            })
            .Build();

        // Act
        var result = services.AddUsersModule(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);

        // Verifica se os serviços foram registrados
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);

        // Deve conseguir construir sem lançar exceções
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddUsersModule_WithEmptyConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act - Não deve lançar exceção durante o registro mesmo com configuração vazia
        var result = services.AddUsersModule(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0, "Services should be registered even with empty configuration");
    }

    [Fact]
    public void AddUsersModule_ShouldReturnSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddUsersModule(configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddUsersModule_ShouldConfigureServicesForDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=test;User Id=test;Password=test;",
                ["Keycloak:BaseUrl"] = "http://localhost:8080",
                ["Keycloak:Realm"] = "test-realm",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:ClientSecret"] = "test-secret"
            })
            .Build();

        // Act
        services.AddUsersModule(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Deve conseguir construir o service provider sem exceções
        Assert.NotNull(serviceProvider);

        // Verifica se alguns serviços básicos estão registrados
        Assert.Contains(services, s => s.ServiceType.Namespace?.Contains("Users") == true);
    }

    [Fact]
    public void AddUsersModule_WithMinimalConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        var result = services.AddUsersModule(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);

        // Deve registrar pelo menos alguns serviços
        Assert.True(services.Count > 0);
    }

    [Theory]
    [InlineData("Server=localhost;Database=test1;", "test-realm")]
    [InlineData("Server=localhost;Database=test2;", "another-realm")]
    [InlineData("", "")]
    public void AddUsersModule_WithVariousConfigurations_ShouldRegisterServices(string connectionString, string realm)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Keycloak:BaseUrl"] = "http://localhost:8080",
                ["Keycloak:Realm"] = realm,
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:ClientSecret"] = "test-secret"
            })
            .Build();

        // Act
        var result = services.AddUsersModule(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddUsersModule_WithCompleteConfiguration_ShouldBuildServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=meajudaai;User Id=postgres;Password=postgres;",
                ["Keycloak:BaseUrl"] = "http://localhost:8080",
                ["Keycloak:Realm"] = "meajudaai",
                ["Keycloak:ClientId"] = "meajudaai-client",
                ["Keycloak:ClientSecret"] = "secret",
                ["Keycloak:AdminUsername"] = "admin",
                ["Keycloak:AdminPassword"] = "admin"
            })
            .Build());

        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        // Act
        services.AddUsersModule(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }
}
