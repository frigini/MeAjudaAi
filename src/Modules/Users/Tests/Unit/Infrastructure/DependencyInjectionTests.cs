using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using MeAjudaAi.Shared.Messaging;
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
    private (IServiceCollection Services, ServiceProvider Provider) BuildProvider(Dictionary<string, string?> settings)
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
        services.AddSingleton(Mock.Of<IMessageBus>());

        services.AddInfrastructure(configuration);
        services.AddLogging();
        
        var provider = services.BuildServiceProvider(new ServiceProviderOptions 
        { 
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        return (services, provider);
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
        var (services, provider) = BuildProvider(settings);
        using (provider)
        {
            using var scope = provider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            // Assert
            scopedProvider.GetRequiredService<UsersDbContext>().Should().NotBeNull();
            scopedProvider.GetRequiredService<IUserRepository>().Should().NotBeNull();
            scopedProvider.GetRequiredService<IUserDomainService>().Should().NotBeNull();
            scopedProvider.GetRequiredService<IAuthenticationDomainService>().Should().NotBeNull();
            
            services.Single(d => d.ServiceType == typeof(IUserRepository)).Lifetime.Should().Be(ServiceLifetime.Scoped);
            services.Single(d => d.ServiceType == typeof(IUserDomainService)).Lifetime.Should().Be(ServiceLifetime.Scoped);
        }
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
        var (services, provider) = BuildProvider(settings);
        using (provider)
        {
            using var scope = provider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            // Assert
            scopedProvider.GetRequiredService<IUserDomainService>().Should().NotBeNull();
            scopedProvider.GetRequiredService<IAuthenticationDomainService>().Should().NotBeNull();
            
            // Verifica que a persistência não foi quebrada ao habilitar Keycloak
            scopedProvider.GetRequiredService<IUserRepository>().Should().NotBeNull();
            scopedProvider.GetRequiredService<UsersDbContext>().Should().NotBeNull();

            services.Single(d => d.ServiceType == typeof(IUserRepository)).Lifetime.Should().Be(ServiceLifetime.Scoped);
        }
    }
}
