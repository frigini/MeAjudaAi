using MeAjudaAi.ApiService.Endpoints;
using MeAjudaAi.Contracts.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Endpoints;

public class ConfigurationEndpointsTests
{
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IWebHostEnvironment> _envMock = new();

    [Fact]
    public void GetClientConfiguration_Should_ReturnValidConfiguration()
    {
        // Arrange
        _configMock.Setup(x => x["ApiBaseUrl"]).Returns("https://api.test.com");
        _configMock.Setup(x => x["Keycloak:Authority"]).Returns("https://keycloak.test.com/realms/test");
        _configMock.Setup(x => x["Keycloak:ClientId"]).Returns("test-client");
        _configMock.Setup(x => x["ClientBaseUrl"]).Returns("https://client.test.com");
        _envMock.Setup(x => x.EnvironmentName).Returns(Environments.Development);

        // Act
        var result = ConfigurationEndpoints.GetClientConfiguration(_configMock.Object, _envMock.Object);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value!.ApiBaseUrl.Should().Be("https://api.test.com");
        result.Value.Keycloak.Authority.Should().Be("https://keycloak.test.com/realms/test");
        result.Value.Keycloak.ClientId.Should().Be("test-client");
        result.Value.Features.EnableReduxDevTools.Should().BeTrue();
    }

    [Fact]
    public void GetClientConfiguration_WithBaseUrlAndRealm_Should_ConstructAuthority()
    {
        // Arrange
        _configMock.Setup(x => x["ApiBaseUrl"]).Returns("https://api.test.com");
        _configMock.Setup(x => x["Keycloak:BaseUrl"]).Returns("https://keycloak.test.com");
        _configMock.Setup(x => x["Keycloak:Realm"]).Returns("myrealm");
        _configMock.Setup(x => x["Keycloak:ClientId"]).Returns("test-client");
        _envMock.Setup(x => x.EnvironmentName).Returns(Environments.Production);

        // Act
        var result = ConfigurationEndpoints.GetClientConfiguration(_configMock.Object, _envMock.Object);

        // Assert
        result.Value!.Keycloak.Authority.Should().Be("https://keycloak.test.com/realms/myrealm");
        result.Value.Features.EnableReduxDevTools.Should().BeFalse();
    }

    [Fact]
    public void GetClientConfiguration_MissingClientId_Should_Throw()
    {
        // Arrange
        _configMock.Setup(x => x["Keycloak:Authority"]).Returns("https://keycloak.test.com");

        // Act
        var act = () => ConfigurationEndpoints.GetClientConfiguration(_configMock.Object, _envMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Keycloak:ClientId*");
    }
}
