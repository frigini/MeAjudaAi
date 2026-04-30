using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class SecurityExtensionsTests
{
    private static IServiceCollection CreateServices() => new ServiceCollection();

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();
    }

    private static IWebHostEnvironment CreateMockEnvironment(string environmentName = "Development")
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns(environmentName);
        return mock.Object;
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullServices_ShouldThrowArgumentNullException()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment();

        var action = () => SecurityExtensions.AddEnvironmentAuthentication(null!, configuration, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        var services = CreateServices();
        var environment = CreateMockEnvironment();

        var action = () => services.AddEnvironmentAuthentication(null!, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration(new Dictionary<string, string?>());

        var action = () => services.AddEnvironmentAuthentication(configuration, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithValidConfig_ShouldNotThrow()
    {
        var services = CreateServices();
        var settings = new Dictionary<string, string?>
        {
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Development");

        var action = () => services.AddEnvironmentAuthentication(configuration, environment);
        action.Should().NotThrow();
    }

    [Fact]
    public void AddKeycloakAuthentication_WithNullServices_ShouldThrowArgumentNullException()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment("Development");

        var action = () => SecurityExtensions.AddKeycloakAuthentication(null!, configuration, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddKeycloakAuthentication_WithValidConfig_ShouldRegisterServices()
    {
        var services = CreateServices();
        var settings = new Dictionary<string, string?>
        {
            ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
            ["Keycloak:Realm"] = "test-realm",
            ["Keycloak:ClientId"] = "test-client",
            ["Keycloak:RequireHttpsMetadata"] = "true"
        };
        var configuration = CreateConfiguration(settings);
        var environment = CreateMockEnvironment("Development");

        var action = () => services.AddKeycloakAuthentication(configuration, environment);
        action.Should().NotThrow();
    }

    [Fact]
    public void AddAuthorizationPolicies_WithNullServices_ShouldThrowArgumentNullException()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment();

        var action = () => SecurityExtensions.AddAuthorizationPolicies(null!, configuration, environment);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAuthorizationPolicies_WithValidConfig_ShouldRegisterServices()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var environment = CreateMockEnvironment("Development");

        var action = () => services.AddAuthorizationPolicies(configuration, environment);
        action.Should().NotThrow();
    }

    [Fact]
    public void AddCustomAntiforgery_WithNullServices_ShouldThrowArgumentNullException()
    {
        var action = () => SecurityExtensions.AddCustomAntiforgery(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCustomAntiforgery_ShouldRegisterAntiforgeryServices()
    {
        var services = CreateServices();

        var action = () => services.AddCustomAntiforgery();
        action.Should().NotThrow();
    }
}