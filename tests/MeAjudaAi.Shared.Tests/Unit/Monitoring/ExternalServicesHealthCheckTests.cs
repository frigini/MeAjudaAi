#pragma warning disable CA2000 // Dispose objects before losing scope - HttpResponseMessage in mocks is disposed by HttpClient
using System.Net;
using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Moq.Protected;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

/// <summary>
/// Testes para ExternalServicesHealthCheck - verifica disponibilidade de serviços externos.
/// Cobre Keycloak, APIs externas, timeout scenarios, error handling.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ExternalServicesHealthCheckTests : IDisposable
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public ExternalServicesHealthCheckTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    #region Keycloak Health Check Tests

    [Fact]
    public async Task CheckHealthAsync_WithHealthyKeycloak_ShouldReturnHealthy()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == "https://keycloak.test/realms/meajudaai"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"realm\":\"meajudaai\"}")
            });

        // Mock IBGE API também
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("ibge.gov.br")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("All external services are operational");
        result.Data.Should().ContainKey("keycloak");
        result.Data.Should().ContainKey("ibge_api");
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("overall_status");
        result.Data["overall_status"].Should().Be("healthy");

        // Verify response time is measured
        var keycloakData = result.Data["keycloak"];
        keycloakData.Should().NotBeNull();
        var keycloakDict = keycloakData.GetType().GetProperty("response_time_ms")?.GetValue(keycloakData);
        keycloakDict.Should().NotBeNull();
        ((long)keycloakDict!).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CheckHealthAsync_WithUnhealthyKeycloak_ShouldReturnDegraded()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("Some external services are not operational");
        result.Data.Should().ContainKey("keycloak");
        result.Data["overall_status"].Should().Be("degraded");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenKeycloakThrowsException_ShouldReturnDegraded()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("keycloak");
        result.Data["keycloak"].Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_WithoutKeycloakConfiguration_ShouldReturnHealthy()
    {
        // Arrange - No Keycloak URL configured
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns((string?)null);

        // Mock IBGE API
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert - Should be healthy when Keycloak is not configured
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data["overall_status"].Should().Be("healthy");
        result.Data.Should().ContainKey("ibge_api");
    }

    #endregion

    #region Timeout and Cancellation Tests

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_ShouldHandleGracefully()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data["overall_status"].Should().Be("degraded");
    }

    [Fact]
    public async Task CheckHealthAsync_WithSlowResponse_ShouldComplete()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(100); // Simulate slow response
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().ContainKey("keycloak");
    }

    #endregion

    #region Data Validation Tests

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeTimestamp()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();
        var beforeCheck = DateTime.UtcNow;

        // Act
        var result = await healthCheck.CheckHealthAsync(context);
        var afterCheck = DateTime.UtcNow;

        // Assert
        result.Data.Should().ContainKey("timestamp");
        var timestamp = (DateTime)result.Data["timestamp"];
        timestamp.Should().BeOnOrAfter(beforeCheck).And.BeOnOrBefore(afterCheck);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeOverallStatus()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns((string?)null);

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().ContainKey("overall_status");
        var status = result.Data["overall_status"] as string;
        status.Should().BeOneOf("healthy", "degraded");
    }

    #endregion

    #region IBGE API Health Check Tests

    [Fact]
    public async Task CheckHealthAsync_WithHealthyIbgeApi_ShouldReturnHealthy()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns((string?)null);
        _configurationMock.Setup(c => c["ExternalServices:IbgeApi:BaseUrl"]).Returns((string?)null);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == "https://servicodados.ibge.gov.br/api/v1/localidades/estados"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[{\"id\":31,\"sigla\":\"MG\",\"nome\":\"Minas Gerais\"}]")
            });

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("ibge_api");
        result.Data["overall_status"].Should().Be("healthy");

        var ibgeData = result.Data["ibge_api"];
        ibgeData.Should().NotBeNull();
        var ibgeStatus = ibgeData.GetType().GetProperty("status")?.GetValue(ibgeData);
        ibgeStatus.Should().Be("healthy");
    }

    [Fact]
    public async Task CheckHealthAsync_WithUnhealthyIbgeApi_ShouldReturnDegraded()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns((string?)null);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("ibge_api");
        result.Data["overall_status"].Should().Be("degraded");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenIbgeApiThrowsException_ShouldReturnDegraded()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns((string?)null);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection timeout"));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("ibge_api");
    }

    [Fact]
    public async Task CheckHealthAsync_WithKeycloakAndIbgeHealthy_ShouldReturnHealthy()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");
        _configurationMock.Setup(c => c["ExternalServices:IbgeApi:BaseUrl"]).Returns((string?)null);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == "https://keycloak.test/realms/meajudaai"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == "https://servicodados.ibge.gov.br/api/v1/localidades/estados"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("keycloak");
        result.Data.Should().ContainKey("ibge_api");
        result.Data["overall_status"].Should().Be("healthy");
    }

    [Fact]
    public async Task CheckHealthAsync_WithKeycloakHealthyAndIbgeUnhealthy_ShouldReturnDegraded()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == "https://keycloak.test/realms/meajudaai"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("ibge.gov.br")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("keycloak");
        result.Data.Should().ContainKey("ibge_api");
        result.Data["overall_status"].Should().Be("degraded");
    }

    #endregion

    #region Multiple Services Tests

    [Fact]
    public async Task CheckHealthAsync_WithKeycloakConfigured_ShouldIncludeKeycloakCheck()
    {
        // Arrange
        _configurationMock.Setup(c => c["Keycloak:BaseUrl"]).Returns("https://keycloak.test");
        // Add more external services in the future

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var healthCheck = CreateHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().ContainKey("keycloak");
        // When more services are added, verify they're all checked
    }

    #endregion

    #region Helper Methods

    private IHealthCheck CreateHealthCheck()
    {
        return new MeAjudaAiHealthChecks.ExternalServicesHealthCheck(_httpClient, _configurationMock.Object);
    }

    #endregion
}
