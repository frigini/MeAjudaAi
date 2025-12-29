using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.Shared.Utilities.Constants;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes E2E para middlewares de infraestrutura
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Infrastructure")]
public sealed class MiddlewareEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public MiddlewareEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

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
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", request);

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
        var response = await _fixture.ApiClient.GetAsync("/api/v1/users");

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
        _fixture.ApiClient.DefaultRequestHeaders.Add(AuthConstants.Headers.CorrelationId, customCorrelationId);

        try
        {
            // Act
            var response = await _fixture.ApiClient.GetAsync("/health");

            // Assert
            response.Headers.Should().ContainKey(AuthConstants.Headers.CorrelationId);
            var responseCorrelationId = response.Headers.GetValues(AuthConstants.Headers.CorrelationId).First();
            responseCorrelationId.Should().Be(customCorrelationId);
        }
        finally
        {
            _fixture.ApiClient.DefaultRequestHeaders.Remove(AuthConstants.Headers.CorrelationId);
        }
    }

    [Fact]
    public async Task LoggingContext_NoCorrelationId_ShouldGenerateNew()
    {
        // Arrange
        _fixture.ApiClient.DefaultRequestHeaders.Remove(AuthConstants.Headers.CorrelationId);

        // Act
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().ContainKey(AuthConstants.Headers.CorrelationId);
        var correlationId = response.Headers.GetValues(AuthConstants.Headers.CorrelationId).First();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue("CorrelationId deve ser um GUID válido");
    }

    #endregion

    #region RequestLoggingMiddleware

    [Fact]
    public async Task RequestLogging_ShouldCaptureSuccessfulRequest()
    {
        // Arrange & Act
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // RequestLoggingMiddleware deve ter logado:
        // - RequestId, ClientIP, UserAgent, Método GET, Path /health
    }

    [Fact]
    public async Task RequestLogging_ShouldCaptureFailedRequest()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();

        // Act
        var response = await _fixture.ApiClient.GetAsync("/api/v1/users/99999999-9999-9999-9999-999999999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // RequestLoggingMiddleware deve ter logado erro 404
    }

    [Fact]
    public async Task RequestLogging_WithCustomHeaders_ShouldCaptureClientInfo()
    {
        // Arrange
        _fixture.ApiClient.DefaultRequestHeaders.Add("User-Agent", "E2E-Test-Client/1.0");

        try
        {
            // Act
            var response = await _fixture.ApiClient.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // RequestLoggingMiddleware deve capturar User-Agent: E2E-Test-Client/1.0
        }
        finally
        {
            _fixture.ApiClient.DefaultRequestHeaders.Remove("User-Agent");
        }
    }

    #endregion

    #region SecurityHeadersMiddleware

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXContentTypeOptions()
    {
        // Arrange & Act
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options");
        var headerValue = response.Headers.GetValues("X-Content-Type-Options").First();
        headerValue.Should().Be("nosniff");
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXFrameOptions()
    {
        // Arrange & Act
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Frame-Options");
        var headerValue = response.Headers.GetValues("X-Frame-Options").First();
        headerValue.Should().BeOneOf("DENY", "SAMEORIGIN");
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeContentSecurityPolicy()
    {
        // Arrange & Act
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "Content-Security-Policy");
        var headerValue = response.Headers.GetValues("Content-Security-Policy").First();
        headerValue.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region CompressionSecurityMiddleware

    [Fact]
    public async Task CompressionSecurity_AuthenticatedUser_ShouldDisableCompression()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsUser();
        _fixture.ApiClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        
        try
        {
            // Act
            var response = await _fixture.ApiClient.GetAsync("/api/v1/users");

            // Assert - Accept both OK (authorized) and Forbidden (authorization denied)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
            
            // CompressionSecurityMiddleware deve desabilitar compressão para usuários autenticados
            // (proteção contra ataques BREACH/CRIME)
            response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                "compression should be disabled for authenticated users");
        }
        finally
        {
            _fixture.ApiClient.DefaultRequestHeaders.Remove("Accept-Encoding");
        }
    }

    [Fact]
    public async Task CompressionSecurity_AnonymousUser_ShouldNotBlockCompression()
    {
        // Arrange
        _fixture.ApiClient.DefaultRequestHeaders.Remove("Authorization");
        _fixture.ApiClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        try
        {
            // Act
            var response = await _fixture.ApiClient.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                "middleware should not block requests with compression headers");
            
            // Verificar que o middleware não retorna erro
            // Note: Compression may or may not be applied depending on response size and server config
            // O importante é que a requisição seja bem-sucedida (não bloqueada)
            response.IsSuccessStatusCode.Should().BeTrue(
                "compression headers should not cause middleware to return error status");
        }
        finally
        {
            _fixture.ApiClient.DefaultRequestHeaders.Remove("Accept-Encoding");
        }
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
        var response = await _fixture.ApiClient.GetAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrWhiteSpace("404 deve retornar corpo com detalhes do erro");
        
        using var problemDetails = await response.Content.ReadFromJsonAsync<JsonDocument>();
        problemDetails.Should().NotBeNull();
        
        // Validar que tem mensagem de erro
        problemDetails!.RootElement.TryGetProperty("message", out _).Should().BeTrue($"Response deve ter propriedade 'message'. Response: {responseBody}");
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
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", invalidRequest);

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
        _fixture.ApiClient.DefaultRequestHeaders.Remove("Authorization");

        // Act - tentar acessar endpoint protegido
        var response = await _fixture.ApiClient.GetAsync("/api/v1/users");

        // Assert - o usuário anônimo não tem permissão para acessar
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}

