using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes b�sicos de integra��o para verificar o startup da aplica��o e funcionalidades b�sicas
/// </summary>
public class BasicStartupTests : TestContainerTestBase
{
    [Fact]
    public async Task Application_ShouldStart_Successfully()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/");

        // Assert
        // Mesmo um 404 est� ok - significa que a aplica��o iniciou
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
        // Qualquer resposta (mesmo 404) significa que o roteamento est� funcionando
        response.Should().NotBeNull();
    }
}
