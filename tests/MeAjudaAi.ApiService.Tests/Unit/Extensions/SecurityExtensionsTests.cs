using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

public class SecurityExtensionsTests
{
    [Fact]
    public void ValidateSecurityConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var environment = Substitute.For<IWebHostEnvironment>();

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
        var configuration = Substitute.For<IConfiguration>();

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
                ["Keycloak:BaseUrl"] = "https://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client"
            })
            .Build();

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Wildcard CORS origin*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithHttpOriginsInProduction_ShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://example.com",
                ["Keycloak:BaseUrl"] = "https://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client"
            })
            .Build();

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*HTTP origins*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithHttpKeycloakInProduction_ShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://example.com",
                ["Keycloak:BaseUrl"] = "http://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client"
            })
            .Build();

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak BaseUrl*HTTPS*production*");
    }

    [Fact]
    public void ValidateSecurityConfiguration_WithWildcardAllowedHostsInProduction_ShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedHosts"] = "*",
                ["Cors:AllowedOrigins:0"] = "https://example.com",
                ["Keycloak:BaseUrl"] = "https://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client"
            })
            .Build();

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*AllowedHosts*production*");
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
                ["Keycloak:BaseUrl"] = "https://keycloak.test",
                ["Keycloak:Realm"] = "test",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:RequireHttpsMetadata"] = "true"
            })
            .Build();

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);

        // Act
        var action = () => SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddCorsPolicy_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = Substitute.For<IConfiguration>();
        var environment = Substitute.For<IWebHostEnvironment>();

        // Act
        var action = () => SecurityExtensions.AddCorsPolicy(null!, configuration, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddCorsPolicy_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();

        // Act
        var action = () => SecurityExtensions.AddCorsPolicy(services, null!, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void AddCorsPolicy_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        // Act
        var action = () => SecurityExtensions.AddCorsPolicy(services, configuration, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("environment");
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = Substitute.For<IConfiguration>();
        var environment = Substitute.For<IWebHostEnvironment>();

        // Act
        var action = () => SecurityExtensions.AddEnvironmentAuthentication(null!, configuration, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();

        // Act
        var action = () => SecurityExtensions.AddEnvironmentAuthentication(services, null!, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void AddEnvironmentAuthentication_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        // Act
        var action = () => SecurityExtensions.AddEnvironmentAuthentication(services, configuration, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("environment");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var configuration = Substitute.For<IConfiguration>();
        var environment = Substitute.For<IWebHostEnvironment>();

        // Act
        var action = () => SecurityExtensions.AddKeycloakAuthentication(null!, configuration, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();

        // Act
        var action = () => SecurityExtensions.AddKeycloakAuthentication(services, null!, environment);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void AddKeycloakAuthentication_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        // Act
        var action = () => SecurityExtensions.AddKeycloakAuthentication(services, configuration, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("environment");
    }
}
