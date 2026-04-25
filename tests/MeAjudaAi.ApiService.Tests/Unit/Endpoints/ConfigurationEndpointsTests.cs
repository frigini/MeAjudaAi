using MeAjudaAi.ApiService.Endpoints;
using MeAjudaAi.Contracts.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Endpoints;

[Trait("Category", "Unit")]
public class ConfigurationEndpointsTests
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();

    [Fact]
    public void GetClientConfiguration_ShouldReturnCorrectConfig()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"ApiBaseUrl", "https://api.test.com"},
            {"Keycloak:Authority", "https://keycloak.test.com/realms/test"},
            {"Keycloak:ClientId", "web-client"},
            {"ClientBaseUrl", "https://client.test.com"},
            {"FeatureFlags:EnableFakeAuth", "true"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);

        // Act
        var method = typeof(ConfigurationEndpoints).GetMethod("GetClientConfiguration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        var result = (Ok<ClientConfiguration>)method!.Invoke(null, new object[] { configuration, _envMock.Object })!;

        // Assert
        result.Value.Should().NotBeNull();
        result.Value!.ApiBaseUrl.Should().Be("https://api.test.com");
        result.Value.Keycloak.Authority.Should().Be("https://keycloak.test.com/realms/test");
        result.Value.Keycloak.ClientId.Should().Be("web-client");
        result.Value.Features.EnableReduxDevTools.Should().BeTrue();
        result.Value.Features.EnableFakeAuth.Should().BeTrue();
    }

    [Theory]
    [InlineData(Environments.Development, true)]
    [InlineData(Environments.Production, false)]
    public void GetClientConfiguration_EnvironmentFlags_ShouldMatch(string environment, bool expected)
    {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"ApiBaseUrl", "https://api.test.com"},
            {"Keycloak:Authority", "https://keycloak.test.com"},
            {"Keycloak:ClientId", "web-client"}
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        _envMock.SetupGet(e => e.EnvironmentName).Returns(environment);

        // Act
        var result = ConfigurationEndpoints.GetClientConfiguration(configuration, _envMock.Object);

        // Assert
        result.Value!.Features.EnableDebugMode.Should().Be(expected);
        result.Value.Features.EnableReduxDevTools.Should().Be(expected);
    }

    [Fact]
    public void GetClientConfiguration_ShouldThrow_WhenClientIdMissing()
    {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"ApiBaseUrl", "https://api.test.com"},
            {"Keycloak:Authority", "https://keycloak.test.com"}
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        // Act
        var act = () => ConfigurationEndpoints.GetClientConfiguration(configuration, _envMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*ClientId*");
    }

    [Fact]
    public void GetClientConfiguration_ShouldThrow_WhenAuthorityAndBaseUrlMissing()
    {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"ApiBaseUrl", "https://api.test.com"},
            {"Keycloak:ClientId", "web-client"}
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        // Act
        var act = () => ConfigurationEndpoints.GetClientConfiguration(configuration, _envMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*BaseUrl*Authority*");
    }

    [Fact]
    public void GetClientConfiguration_ShouldNormalizeTrailingSlashes()
    {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"ApiBaseUrl", "https://api.test.com/"},
            {"Keycloak:Authority", "https://keycloak.test.com/"},
            {"Keycloak:ClientId", "web-client"},
            {"ClientBaseUrl", "https://client.test.com/"}
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        _envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        var result = ConfigurationEndpoints.GetClientConfiguration(configuration, _envMock.Object);

        // Assert
        result.Value!.ApiBaseUrl.Should().Be("https://api.test.com");
        result.Value.Keycloak.Authority.Should().Be("https://keycloak.test.com");
        result.Value.Keycloak.PostLogoutRedirectUri.Should().Be("https://client.test.com/");
    }

    [Fact]
    public void GetClientConfiguration_ShouldFallback_ToDefaultApiUrl()
    {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"Keycloak:Authority", "https://keycloak.test.com"},
            {"Keycloak:ClientId", "web-client"}
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        // Act
        var result = ConfigurationEndpoints.GetClientConfiguration(configuration, _envMock.Object);

        // Assert
        result.Value!.ApiBaseUrl.Should().Be("https://localhost:7001");
    }
}
