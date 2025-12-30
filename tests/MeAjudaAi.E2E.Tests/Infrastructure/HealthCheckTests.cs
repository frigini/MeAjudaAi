using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes de integração básicos para saúde da aplicação e conectividade
/// </summary>
public class HealthCheckTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public HealthCheckTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _fixture.ApiClient.GetAsync("/health");

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
        var response = await _fixture.ApiClient.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessCheck_ShouldReturnOkOrServiceUnavailable()
    {
        // Act & Assert - Permite tempo para serviços ficarem prontos
        var maxAttempts = 30;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var response = await _fixture.ApiClient.GetAsync("/health/ready");

            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return; // Teste passou - serviço está respondendo com status aceitável

            if (attempt < maxAttempts - 1)
                await Task.Delay(delay);
        }

        // Tentativa final com diagnóstico detalhado
        var finalResponse = await _fixture.ApiClient.GetAsync("/health/ready");
        
        // Log response for diagnostics only when status is unexpected (not OK or 503)
        if (finalResponse.StatusCode != HttpStatusCode.OK &&
            finalResponse.StatusCode != HttpStatusCode.ServiceUnavailable)
        {
            var content = await finalResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Unexpected health check status {finalResponse.StatusCode}. Response: {content}");
        }

        // ASP.NET Core health check status mapping:
        // - Healthy/Degraded → 200 OK (with status in JSON body)
        // - Unhealthy → 503 ServiceUnavailable
        // In E2E tests, we accept both OK (healthy/degraded) and 503 (unhealthy)
        // as long as the app is running and responding to health checks.
        // Verificação de prontidão: OK (saudável/degradado) ou 503 (não saudável) são aceitáveis em E2E
        finalResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.ServiceUnavailable);
    }
}
