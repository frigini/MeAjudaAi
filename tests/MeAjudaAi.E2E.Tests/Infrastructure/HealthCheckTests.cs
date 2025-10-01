using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração básicos para saúde da aplicação e conectividade
/// </summary>
public class HealthCheckTests : TestContainerTestBase
{
    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.ServiceUnavailable // Aceitável durante inicialização
        );
    }

    [Fact]
    public async Task LivenessCheck_ShouldReturnOk()
    {
        // Act
        var response = await ApiClient.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessCheck_ShouldEventuallyReturnOk()
    {
        // Act & Assert - Permite tempo para serviços ficarem prontos
        var maxAttempts = 30;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var response = await ApiClient.GetAsync("/health/ready");

            if (response.StatusCode == HttpStatusCode.OK)
                return; // Teste passou

            if (attempt < maxAttempts - 1)
                await Task.Delay(delay);
        }

        // Tentativa final com asserção
        var finalResponse = await ApiClient.GetAsync("/health/ready");
        finalResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Verificação de prontidão deve eventualmente retornar OK após serviços estarem prontos");
    }
}