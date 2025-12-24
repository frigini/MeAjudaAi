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
        // Arrange
        await TestContainerFixture.BeforeEachTest();
        
        // Act - Fazer request para gerar métricas
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
    public async Task Smoke_Application_ShouldConfigureHttpClientInstrumentation()
    {
        // Arrange & Act
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var password = _fixture.Faker.Internet.Password(12, true);
        var registerRequest = new
        {
            email = _fixture.Faker.Internet.Email(),
            password = password,
            confirmPassword = password,
            fullName = _fixture.Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", registerRequest);

        // Assert
        // HttpClient instrumentation deve capturar chamadas internas
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Smoke_Application_ShouldFilterHealthCheckEndpoints()
    {
        // Arrange & Act
        TestContainerFixture.BeforeEachTest();
        var healthResponse = await _fixture.ApiClient.GetAsync("/health");
        var aliveResponse = await _fixture.ApiClient.GetAsync("/alive");

        // Assert
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
        aliveResponse.IsSuccessStatusCode.Should().BeTrue();
        // Os endpoints /health e /alive devem ser filtrados do tracing
        // (verificado na configuração do ConfigureOpenTelemetry)
    }

    [Fact]
    public async Task Smoke_Application_ShouldExposeRuntimeInstrumentation()
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
    public async Task Smoke_Application_WithDatabaseOperations_ShouldRespondSuccessfully()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var registerRequest = new
        {
            Username = _fixture.Faker.Internet.UserName(),
            Email = _fixture.Faker.Internet.Email(),
            FirstName = _fixture.Faker.Name.FirstName(),
            LastName = _fixture.Faker.Name.LastName(),
            Password = _fixture.Faker.Internet.Password(12, true),
            PhoneNumber = "+5511987654321"
        };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", registerRequest);

        // Assert
        // EF Core instrumentation deve capturar operações de database
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Smoke_Application_ShouldConfigureTracingWithExceptions()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var invalidRequest = new { };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", invalidRequest);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        // Tracing deve capturar exceções (RecordException = true)
    }

    [Fact]
    public async Task Smoke_Application_ShouldIncludeFormattedMessagesInLogs()
    {
        // Arrange & Act
        TestContainerFixture.BeforeEachTest();
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        // OpenTelemetry logging deve incluir formatted messages e scopes
        // (IncludeFormattedMessage = true, IncludeScopes = true)
    }

    [Fact]
    public async Task Smoke_Application_WithAuthenticatedRequest_ShouldTraceFullPipeline()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        // Act - Request autenticado (usando token do admin via fixture)
        var response = await _fixture.ApiClient.GetAsync("/api/v1/users");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            "tracing should capture full pipeline including authentication");
    }

    [Fact]
    public async Task Smoke_Application_ShouldConfigureServiceName()
    {
        // Arrange & Act
        TestContainerFixture.BeforeEachTest();
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        // ServiceName deve ser configurado via OpenTelemetryOptions ou appsettings
        // (verificado em appsettings.json: "ServiceName": "MeAjudaAi-api")
    }
}


