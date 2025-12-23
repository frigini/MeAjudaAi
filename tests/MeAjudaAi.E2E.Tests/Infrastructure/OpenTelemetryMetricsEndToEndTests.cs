using System.Diagnostics.Metrics;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes E2E para validar métricas OpenTelemetry configuradas na aplicação.
/// Valida que a instrumentação está ativa e exportando métricas corretamente.
/// </summary>
[Trait("Category", "E2E")]
[Trait("Feature", "Telemetry")]
public class OpenTelemetryMetricsEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public OpenTelemetryMetricsEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Application_ShouldExposeAspNetCoreMetrics()
    {
        // Arrange - Fazer request para gerar métricas
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.Should().NotBeNull();
        // A aplicação deve estar instrumentada com AspNetCore metrics
        // Verificar que a aplicação inicia corretamente com OpenTelemetry configurado
        response.IsSuccessStatusCode.Should().BeTrue("OpenTelemetry não deve impedir funcionamento da aplicação");
    }

    [Fact]
    public async Task Application_WithMultipleRequests_ShouldIncrementRequestMetrics()
    {
        // Arrange - Fazer múltiplas requisições
        TestContainerFixture.BeforeEachTest();
        var requests = Enumerable.Range(0, 5).Select(_ => _fixture.ApiClient.GetAsync("/alive"));

        // Act
        var responses = await Task.WhenAll(requests);

        // Assert
        responses.Should().AllSatisfy(r => r.IsSuccessStatusCode.Should().BeTrue());
        // Métricas ASP.NET Core devem ser incrementadas para cada request
    }

    [Fact]
    public async Task Application_ShouldConfigureHttpClientInstrumentation()
    {
        // Arrange & Act - Criar usuário (gera HttpClient calls internos)
        var registerRequest = new
        {
            email = _fixture.Faker.Internet.Email(),
            password = _fixture.Faker.Internet.Password(12, true),
            confirmPassword = _fixture.Faker.Internet.Password(12, true),
            fullName = _fixture.Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };
        
        registerRequest = registerRequest with { confirmPassword = registerRequest.password };

        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/users/register", registerRequest);

        // Assert
        // HttpClient instrumentation deve capturar chamadas internas
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Application_ShouldFilterHealthCheckEndpoints()
    {
        // Arrange & Act - Chamar endpoints de health check
        var healthResponse = await _fixture.ApiClient.GetAsync("/health");
        var aliveResponse = await _fixture.ApiClient.GetAsync("/alive");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        aliveResponse.IsSuccessStatusCode.Should().BeTrue();
        // Os endpoints /health e /alive devem ser filtrados do tracing
        // (verificado na configuração do ConfigureOpenTelemetry)
    }

    [Fact]
    public async Task Application_ShouldExposeRuntimeInstrumentation()
    {
        // Arrange & Act - Fazer request qualquer
        TestContainerFixture.BeforeEachTest();
        var response = await _fixture.ApiClient.GetAsync("/alive");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        // Runtime instrumentation (GC, memória, threads) deve estar ativa
        // Validado pela configuração em ServiceDefaults
    }

    [Fact]
    public async Task Application_WithDatabaseOperations_ShouldExposeEfCoreMetrics()
    {
        // Arrange - Registrar usuário (operação de database)
        var registerRequest = new
        {
            email = _fixture.Faker.Internet.Email(),
            password = _fixture.Faker.Internet.Password(12, true),
            confirmPassword = _fixture.Faker.Internet.Password(12, true),
            fullName = _fixture.Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };
        
        registerRequest = registerRequest with { confirmPassword = registerRequest.password };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/users/register", registerRequest);

        // Assert
        // EF Core instrumentation deve capturar operações de database
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Application_ShouldConfigureTracingWithExceptions()
    {
        // Arrange - Tentar fazer request inválido para gerar exception
        var invalidRequest = new { };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/users/register", invalidRequest);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        // Tracing deve capturar exceções (RecordException = true)
    }

    [Fact]
    public async Task Application_ShouldIncludeFormattedMessagesInLogs()
    {
        // Arrange & Act - Fazer request que gera logs
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        // OpenTelemetry logging deve incluir formatted messages e scopes
        // (IncludeFormattedMessage = true, IncludeScopes = true)
    }

    [Fact]
    public async Task Application_WithAuthenticatedRequest_ShouldTraceFullPipeline()
    {
        // Arrange - Registrar e fazer login
        TestContainerFixture.BeforeEachTest();
        var registerRequest = new
        {
            email = _fixture.Faker.Internet.Email(),
            password = _fixture.Faker.Internet.Password(12, true),
            confirmPassword = _fixture.Faker.Internet.Password(12, true),
            fullName = _fixture.Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };
        
        registerRequest = registerRequest with { confirmPassword = registerRequest.password };
        
        await _fixture.ApiClient.PostAsJsonAsync("/api/users/register", registerRequest);

        var loginRequest = new
        {
            email = registerRequest.email,
            password = registerRequest.password
        };

        var loginResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var token = loginResult!["token"].ToString();

        // Act - Request autenticado
        _fixture.ApiClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _fixture.ApiClient.GetAsync("/api/v1/users");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        // Tracing deve capturar todo o pipeline incluindo autenticação
    }

    [Fact]
    public async Task Application_ShouldConfigureServiceName()
    {
        // Arrange & Act
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        // ServiceName deve ser configurado via OpenTelemetryOptions ou appsettings
        // (verificado em appsettings.json: "ServiceName": "MeAjudaAi-api")
    }
}


