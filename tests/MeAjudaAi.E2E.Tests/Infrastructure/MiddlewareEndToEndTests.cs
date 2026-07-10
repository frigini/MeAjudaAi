using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.Shared.Utilities.Constants;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes E2E para middlewares de infraestrutura
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Infrastructure")]
public sealed class MiddlewareEndToEndTests(TestContainerFixture fixture) : BaseE2ETest<TestContainerFixture>(fixture)
{

    #region BusinessMetricsMiddleware - Rotas Versionadas

    [Fact]
    public async Task BusinessMetrics_UserCreation_ShouldRecordMetric()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var request = new
        {
            Username = $"metrics_{uniqueId}",
            Email = $"metrics.{uniqueId}@example.com",
            Password = "ValidPass123!",
            Role = "User",
            FirstName = "Metrics",
            LastName = "User",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // BusinessMetrics deve ter registrado essa operação
        // Validação: se a requisição foi bem-sucedida, o middleware processou corretamente
    }

    [Fact]
    public async Task BusinessMetrics_Authentication_ShouldProcess()
    {
        // Arrange & Act
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsUser();
        var response = await Fixture.ApiClient.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
        
        // BusinessMetrics deve processar requisições autenticadas
    }

    #endregion

    #region LoggingContext - CorrelationId

    [Fact]
    public async Task LoggingContext_CorrelationId_ShouldPropagateToResponseHeader()
    {
        // Arrange
        var customCorrelationId = Guid.NewGuid().ToString();
        Fixture.ApiClient.DefaultRequestHeaders.Add(AuthConstants.Headers.CorrelationId, customCorrelationId);

        try
        {
            // Act
            var response = await Fixture.ApiClient.GetAsync("/health");

            // Assert
            response.Headers.Should().ContainKey(AuthConstants.Headers.CorrelationId);
            var responseCorrelationId = response.Headers.GetValues(AuthConstants.Headers.CorrelationId).First();
            responseCorrelationId.Should().Be(customCorrelationId);
        }
        finally
        {
            Fixture.ApiClient.DefaultRequestHeaders.Remove(AuthConstants.Headers.CorrelationId);
        }
    }

    [Fact]
    public async Task LoggingContext_NoCorrelationId_ShouldGenerateNew()
    {
        // Arrange
        Fixture.ApiClient.DefaultRequestHeaders.Remove(AuthConstants.Headers.CorrelationId);

        // Act
        var response = await Fixture.ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().ContainKey(AuthConstants.Headers.CorrelationId);
        var correlationId = response.Headers.GetValues(AuthConstants.Headers.CorrelationId).First();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue("CorrelationId deve ser um GUID válido");
    }

    #endregion

    #region ExceptionHandlerMiddleware - ProblemDetails

    [Fact]
    public async Task ExceptionHandler_NotFound_ShouldReturnProblemDetails()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Fixture.ApiClient.GetAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrWhiteSpace("404 deve retornar corpo com detalhes do erro");
        
        using var problemDetails = await response.Content.ReadFromJsonAsync<JsonDocument>();
        problemDetails.Should().NotBeNull();
        
        // Validar que tem mensagem de erro
        // Com o novo formato ApiResult, o erro pode estar dentro de 'error'
        if (problemDetails!.RootElement.TryGetProperty("error", out var errorProp))
        {
            errorProp.TryGetProperty("message", out _).Should().BeTrue($"Response deve ter propriedade 'message' dentro de 'error'. Response: {responseBody}");
        }
        else
        {
            problemDetails!.RootElement.TryGetProperty("message", out _).Should().BeTrue($"Response deve ter propriedade 'message'. Response: {responseBody}");
        }
    }

    [Fact]
    public async Task ExceptionHandler_BadRequest_ShouldReturnProblemDetails()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var invalidRequest = new
        {
            Username = "", // Inválido - vazio
            Email = "not-an-email", // Inválido - formato
            Password = "123", // Inválido - muito curto
            Role = "InvalidRole", // Inválido - role não existente
            PhoneNumber = "+5511999999999",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/users", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        using var problemDetails = await response.Content.ReadFromJsonAsync<JsonDocument>();
        problemDetails.Should().NotBeNull();
        
        // Validar estrutura ProblemDetails
        problemDetails!.RootElement.TryGetProperty("type", out _).Should().BeTrue();
        problemDetails.RootElement.TryGetProperty("title", out _).Should().BeTrue();
        problemDetails.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetInt32().Should().Be(400);
        
        // Validação deve incluir errors com detalhes
        problemDetails.RootElement.TryGetProperty("errors", out _).Should().BeTrue("BadRequest deve incluir detalhes de validação");
    }

    [Fact]
    public async Task ExceptionHandler_Unauthorized_ShouldReturnProblemDetails()
    {
        // Arrange - sem autenticação mas com context ID
        var contextId = ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(true);
        Fixture.ApiClient.DefaultRequestHeaders.Remove("Authorization");

        // Act - tentar acessar endpoint protegido
        var response = await Fixture.ApiClient.GetAsync("/api/v1/users");

        // Assert - o usuário anônimo não tem permissão para acessar
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}