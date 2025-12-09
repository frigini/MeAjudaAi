using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

/// <summary>
/// Unit tests for <see cref="SecurityExtensions"/> validating security configuration,
/// CORS policies, authentication, and authorization setup.
/// </summary>
public sealed class SecurityExtensionsTests
{
    private static IWebHostEnvironment CreateEnvironment(string environmentName)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(x => x.EnvironmentName).Returns(environmentName);
        return mock.Object;
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var environment = CreateEnvironment(Environments.Development);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(null!, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>().Object;

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("environment");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithWildcardCorsInProduction_ShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "*",
                ["Cors:AllowedMethods:0"] = "GET",
                ["Cors:AllowedHeaders:0"] = "Content-Type",
                ["Keycloak:BaseUrl"] = "https://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client"
            })
            .Build();

        var environment = CreateEnvironment(Environments.Production);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Wildcard CORS origin*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithMissingKeycloakBaseUrl_ShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://example.com",
                ["Cors:AllowedMethods:0"] = "GET",
                ["Cors:AllowedHeaders:0"] = "Content-Type",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client"
            })
            .Build();

        var environment = CreateEnvironment(Environments.Development);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak BaseUrl*required*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedHosts"] = "example.com",
                ["Cors:AllowedOrigins:0"] = "https://example.com",
                ["Cors:AllowedMethods:0"] = "GET",
                ["Cors:AllowedHeaders:0"] = "Content-Type",
                ["Keycloak:BaseUrl"] = "https://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:RequireHttpsMetadata"] = "true"
            })
            .Build();

        var environment = CreateEnvironment(Environments.Production);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddCorsPolicy_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>().Object;
        var environment = CreateEnvironment(Environments.Development);

        // Act
        var action = () => SecurityExtensions.AddCorsPolicy(null!, configuration, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddCorsPolicy_WithSpecificOrigins_ShouldConfigureCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://example.com",
                ["Cors:AllowedOrigins:1"] = "https://test.com",
                ["Cors:AllowedMethods:0"] = "GET",
                ["Cors:AllowedHeaders:0"] = "Content-Type",
                ["Cors:AllowCredentials"] = "false",
                ["Cors:PreflightMaxAge"] = "3600"
            })
            .Build();

        var environment = CreateEnvironment(Environments.Production);

        // Act
        var result = services.AddCorsPolicy(configuration, environment);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAuthorizationPolicies_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Act
        var action = () => SecurityExtensions.AddAuthorizationPolicies(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAuthorizationPolicies_ShouldRegisterRequiredPoliciesAndHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAuthorizationPolicies();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(sd => sd.ServiceType == typeof(IAuthorizationHandler));
    }

    [Fact]
    public void AddKeycloakAuthentication_WithValidConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:BaseUrl"] = "https://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client"
            })
            .Build();

        var environment = CreateEnvironment(Environments.Development);

        // Act
        var result = services.AddKeycloakAuthentication(configuration, environment);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(sd => sd.ServiceType == typeof(IHostedService));
    }
}
