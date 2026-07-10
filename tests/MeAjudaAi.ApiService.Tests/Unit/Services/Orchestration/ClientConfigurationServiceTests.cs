using MeAjudaAi.ApiService.Services.Orchestration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Tests.Unit.Services.Orchestration;

public class ClientConfigurationServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IHostEnvironment> _envMock;
    private readonly Mock<ILogger<ClientConfigurationService>> _loggerMock;

    public ClientConfigurationServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _envMock = new Mock<IHostEnvironment>();
        _loggerMock = new Mock<ILogger<ClientConfigurationService>>();
    }

    [Fact]
    public void GetClientConfiguration_WithAllConfig_ShouldReturnValidConfiguration()
    {
        // Arrange
        _configMock.Setup(x => x["ApiBaseUrl"]).Returns("https://api.test.com");
        _configMock.Setup(x => x["Keycloak:Authority"]).Returns("https://keycloak.test.com/realms/test");
        _configMock.Setup(x => x["Keycloak:ClientId"]).Returns("test-client");
        _configMock.Setup(x => x["ClientBaseUrl"]).Returns("https://client.test.com");
        _envMock.Setup(x => x.EnvironmentName).Returns("Development");
        var service = new ClientConfigurationService(_configMock.Object, _envMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetClientConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.ApiBaseUrl.Should().Be("https://api.test.com");
        result.Keycloak.Authority.Should().Be("https://keycloak.test.com/realms/test");
        result.Keycloak.ClientId.Should().Be("test-client");
        result.Features.EnableReduxDevTools.Should().BeTrue();
    }

    [Fact]
    public void GetClientConfiguration_WithBaseUrlAndRealm_ShouldConstructAuthority()
    {
        // Arrange
        _configMock.Setup(x => x["ApiBaseUrl"]).Returns("https://api.test.com");
        _configMock.Setup(x => x["Keycloak:BaseUrl"]).Returns("https://keycloak.test.com");
        _configMock.Setup(x => x["Keycloak:Realm"]).Returns("myrealm");
        _configMock.Setup(x => x["Keycloak:ClientId"]).Returns("test-client");
        _envMock.Setup(x => x.EnvironmentName).Returns("Production");
        var service = new ClientConfigurationService(_configMock.Object, _envMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetClientConfiguration();

        // Assert
        result.Keycloak.Authority.Should().Be("https://keycloak.test.com/realms/myrealm");
        result.Features.EnableReduxDevTools.Should().BeFalse();
    }

    [Fact]
    public void GetClientConfiguration_WithDefaultRealm_ShouldUseMeajudaai()
    {
        // Arrange
        _configMock.Setup(x => x["ApiBaseUrl"]).Returns("https://api.test.com");
        _configMock.Setup(x => x["Keycloak:BaseUrl"]).Returns("https://keycloak.test.com");
        _configMock.Setup(x => x["Keycloak:ClientId"]).Returns("test-client");
        _envMock.Setup(x => x.EnvironmentName).Returns("Production");
        var service = new ClientConfigurationService(_configMock.Object, _envMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetClientConfiguration();

        // Assert
        result.Keycloak.Authority.Should().Be("https://keycloak.test.com/realms/meajudaai");
    }

    [Fact]
    public void GetClientConfiguration_MissingClientId_ShouldThrow()
    {
        // Arrange
        _configMock.Setup(x => x["Keycloak:Authority"]).Returns("https://keycloak.test.com");
        _envMock.Setup(x => x.EnvironmentName).Returns("Production");
        var service = new ClientConfigurationService(_configMock.Object, _envMock.Object, _loggerMock.Object);

        // Act
        var act = () => service.GetClientConfiguration();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*ClientId*");
    }

    [Fact]
    public void GetClientConfiguration_MissingKeycloakConfig_ShouldThrow()
    {
        // Arrange
        _envMock.Setup(x => x.EnvironmentName).Returns("Production");
        var service = new ClientConfigurationService(_configMock.Object, _envMock.Object, _loggerMock.Object);

        // Act
        var act = () => service.GetClientConfiguration();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Keycloak*");
    }

    [Fact]
    public void GetClientConfiguration_WithTrailingSlash_ShouldNormalize()
    {
        // Arrange
        _configMock.Setup(x => x["ApiBaseUrl"]).Returns("https://api.test.com/");
        _configMock.Setup(x => x["Keycloak:Authority"]).Returns("https://keycloak.test.com/realms/test/");
        _configMock.Setup(x => x["Keycloak:ClientId"]).Returns("test-client");
        _configMock.Setup(x => x["ClientBaseUrl"]).Returns("https://client.test.com/");
        _envMock.Setup(x => x.EnvironmentName).Returns("Production");
        var service = new ClientConfigurationService(_configMock.Object, _envMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetClientConfiguration();

        // Assert
        result.ApiBaseUrl.Should().Be("https://api.test.com");
        result.Keycloak.Authority.Should().Be("https://keycloak.test.com/realms/test");
        result.Keycloak.PostLogoutRedirectUri.Should().Be("https://client.test.com/");
    }
}
