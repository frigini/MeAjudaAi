using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?> {
            {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=test;Username=postgres;Password=test"},
            {"Keycloak:Enabled", "false"} // Força o uso de mocks
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        services.AddSingleton(envMock.Object);
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddInfrastructure(configuration);
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<UsersDbContext>().Should().NotBeNull();
        provider.GetRequiredService<IUserRepository>().Should().NotBeNull();
        provider.GetRequiredService<IUserDomainService>().Should().NotBeNull();
        provider.GetRequiredService<IAuthenticationDomainService>().Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_WithKeycloakEnabled_ShouldRegisterKeycloakServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?> {
            {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=test;Username=postgres;Password=test"},
            {"Keycloak:Enabled", "true"},
            {"Keycloak:BaseUrl", "https://keycloak.test.com"},
            {"Keycloak:Realm", "master"},
            {"Keycloak:ClientId", "admin-cli"},
            {"Keycloak:ClientSecret", "secret"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        services.AddSingleton(envMock.Object);
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddInfrastructure(configuration);
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IUserDomainService>().Should().BeOfType<MeAjudaAi.Modules.Users.Infrastructure.Services.KeycloakUserDomainService>();
    }
}
