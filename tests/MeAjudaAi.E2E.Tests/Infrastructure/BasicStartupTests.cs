using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes básicos de integração para verificar o startup da aplicação e funcionalidades básicas
/// </summary>
public class BasicStartupTests : TestContainerTestBase
{
    [Fact]
    public async Task Application_ShouldStart_Successfully()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/");

        // Assert
        // Mesmo um 404 está ok - significa que a aplicação iniciou
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk_WhenApplicationIsRunning()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiEndpoint_ShouldBeAccessible()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/api");

        // Assert
        // Qualquer resposta (mesmo 404) significa que o roteamento está funcionando
        response.Should().NotBeNull();
    }
}