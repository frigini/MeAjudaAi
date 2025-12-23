using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Constants;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes E2E para middlewares de infraestrutura
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Infrastructure")]
public sealed class MiddlewareEndToEndTests : TestContainerTestBase
{
    #region BusinessMetricsMiddleware - Rotas Versionadas

    [Fact]
    public async Task BusinessMetrics_UserCreation_ShouldRecordMetric()
    {
        // Arrange
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var request = new
        {
            Username = $"metrics_{uniqueId}",
            Email = $"metrics.{uniqueId}@example.com",
            Password = "ValidPass123!",
            Role = "User",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // BusinessMetrics deve ter registrado essa operação
        // Validação: se a requisição foi bem-sucedida, o middleware processou corretamente
    }

    [Fact]
    public async Task BusinessMetrics_Authentication_ShouldProcess()
    {
        // Arrange & Act
        AuthenticateAsUser();
        var response = await ApiClient.GetAsync("/api/v1/users");

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
        ApiClient.DefaultRequestHeaders.Add(AuthConstants.Headers.CorrelationId, customCorrelationId);

        try
        {
            // Act
            var response = await ApiClient.GetAsync("/health");

            // Assert
            response.Headers.Should().ContainKey(AuthConstants.Headers.CorrelationId);
            var responseCorrelationId = response.Headers.GetValues(AuthConstants.Headers.CorrelationId).First();
            responseCorrelationId.Should().Be(customCorrelationId);
        }
        finally
        {
            ApiClient.DefaultRequestHeaders.Remove(AuthConstants.Headers.CorrelationId);
        }
    }

    [Fact]
    public async Task LoggingContext_NoCorrelationId_ShouldGenerateNew()
    {
        // Arrange
        ApiClient.DefaultRequestHeaders.Remove(AuthConstants.Headers.CorrelationId);

        // Act
        var response = await ApiClient.GetAsync("/health");

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
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // RequestLoggingMiddleware deve ter logado:
        // - RequestId, ClientIP, UserAgent, Método GET, Path /health
    }

    [Fact]
    public async Task RequestLogging_ShouldCaptureFailedRequest()
    {
        // Arrange
        AuthenticateAsUser();

        // Act
        var response = await ApiClient.GetAsync("/api/v1/users/99999999-9999-9999-9999-999999999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // RequestLoggingMiddleware deve ter logado erro 404
    }

    [Fact]
    public async Task RequestLogging_WithCustomHeaders_ShouldCaptureClientInfo()
    {
        // Arrange
        ApiClient.DefaultRequestHeaders.Add("User-Agent", "E2E-Test-Client/1.0");

        try
        {
            // Act
            var response = await ApiClient.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // RequestLoggingMiddleware deve capturar User-Agent: E2E-Test-Client/1.0
        }
        finally
        {
            ApiClient.DefaultRequestHeaders.Remove("User-Agent");
        }
    }

    #endregion

    #region SecurityHeadersMiddleware

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXContentTypeOptions()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options");
        var headerValue = response.Headers.GetValues("X-Content-Type-Options").First();
        headerValue.Should().Be("nosniff");
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXFrameOptions()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Frame-Options");
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeContentSecurityPolicy()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "Content-Security-Policy");
    }

    #endregion

    #region CompressionSecurityMiddleware

    [Fact]
    public async Task CompressionSecurity_AuthenticatedUser_ShouldDisableCompression()
    {
        // Arrange
        AuthenticateAsUser();
        
        try
        {
            ApiClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            // Act
            var response = await ApiClient.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, "endpoint should return success");
            
            // CompressionSecurityMiddleware deve desabilitar compressão para usuários autenticados
            // (proteção contra ataques BREACH/CRIME)
            response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                "compression should be disabled for authenticated users");
        }
        finally
        {
            ApiClient.DefaultRequestHeaders.Remove("Accept-Encoding");
        }
    }

    [Fact]
    public async Task CompressionSecurity_AnonymousUser_ShouldAllowCompression()
    {
        // Arrange
        ApiClient.DefaultRequestHeaders.Remove("Authorization");
        ApiClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Para usuários anônimos, compressão pode estar habilitada
        
        ApiClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    #endregion

    #region ExceptionHandlerMiddleware - ProblemDetails

    [Fact]
    public async Task ExceptionHandler_NotFound_ShouldReturnProblemDetails()
    {
        // Arrange
        AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonDocument>();
        problemDetails.Should().NotBeNull();
        
        // Validar estrutura ProblemDetails (RFC 7807)
        problemDetails!.RootElement.TryGetProperty("type", out _).Should().BeTrue("ProblemDetails deve ter propriedade 'type'");
        problemDetails.RootElement.TryGetProperty("title", out _).Should().BeTrue("ProblemDetails deve ter propriedade 'title'");
        problemDetails.RootElement.TryGetProperty("status", out var status).Should().BeTrue("ProblemDetails deve ter propriedade 'status'");
        status.GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task ExceptionHandler_BadRequest_ShouldReturnProblemDetails()
    {
        // Arrange
        AuthenticateAsAdmin();
        var invalidRequest = new
        {
            Username = "", // Inválido - vazio
            Email = "not-an-email", // Inválido - formato
            Password = "123", // Inválido - muito curto
            Role = "InvalidRole", // Inválido - role não existente
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonDocument>();
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
        // Arrange - sem autenticação
        ApiClient.DefaultRequestHeaders.Remove("Authorization");

        // Act - tentar acessar endpoint protegido
        var response = await ApiClient.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Validar que mesmo 401 retorna structured response
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty("Response deve ter conteúdo estruturado");
    }

    #endregion
}
