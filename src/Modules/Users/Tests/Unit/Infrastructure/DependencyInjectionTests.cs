using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
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
    private IServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        services.AddSingleton(envMock.Object);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(configuration);

        services.AddInfrastructure(configuration);
        services.AddLogging();
        
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterRequiredServices()
    {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=test;Username=postgres;Password=test"},
            {"Keycloak:Enabled", "false"}
        };

        // Act
        var provider = BuildProvider(settings);

        // Assert
        provider.GetRequiredService<UsersDbContext>().Should().NotBeNull();
        provider.GetRequiredService<IUserRepository>().Should().NotBeNull();
        provider.GetRequiredService<IUserDomainService>().Should().BeOfType<LocalDevelopmentUserDomainService>();
        provider.GetRequiredService<IAuthenticationDomainService>().Should().BeOfType<LocalDevelopmentUserDomainService>();
    }

    [Fact]
    public void AddInfrastructure_WithKeycloakEnabled_ShouldRegisterKeycloakServices()
    {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=test;Username=postgres;Password=test"},
            {"Keycloak:Enabled", "true"},
            {"Keycloak:BaseUrl", "https://keycloak.test.com"},
            {"Keycloak:Realm", "master"},
            {"Keycloak:ClientId", "admin-cli"},
            {"Keycloak:ClientSecret", "secret"}
        };

        // Act
        var provider = BuildProvider(settings);

        // Assert
        provider.GetRequiredService<IUserDomainService>().Should().BeOfType<KeycloakUserDomainService>();
        provider.GetRequiredService<IAuthenticationDomainService>().Should().BeOfType<KeycloakUserDomainService>();
    }
}
