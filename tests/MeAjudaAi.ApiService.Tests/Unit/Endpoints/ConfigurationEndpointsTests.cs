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

    [Fact]
    public void GetClientConfiguration_WithBaseUrlAndRealm_ShouldConstructAuthority()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"ApiBaseUrl", "https://api.test.com"},
            {"Keycloak:BaseUrl", "https://auth.test.com"},
            {"Keycloak:Realm", "myrealm"},
            {"Keycloak:ClientId", "web-client"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        var method = typeof(ConfigurationEndpoints).GetMethod("GetClientConfiguration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        var result = (Ok<ClientConfiguration>)method!.Invoke(null, new object[] { configuration, _envMock.Object })!;

        // Assert
        result.Value!.Keycloak.Authority.Should().Be("https://auth.test.com/realms/myrealm");
        result.Value.Features.EnableReduxDevTools.Should().BeFalse();
    }
}
