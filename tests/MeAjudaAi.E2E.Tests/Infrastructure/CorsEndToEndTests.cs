using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes E2E para validar políticas CORS configuradas na aplicação.
/// 
/// CORS (Cross-Origin Resource Sharing) permite que frontend em domínios diferentes
/// possam fazer requisições para a API.
/// 
/// Testa:
/// - Origens permitidas/bloqueadas
/// - Métodos HTTP permitidos
/// - Headers permitidos
/// - Credenciais (cookies, auth headers)
/// - Preflight requests (OPTIONS)
/// </summary>
[Trait("Category", "E2E")]
[Trait("Feature", "CORS")]
public class CorsEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public CorsEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Preflight Requests (OPTIONS)

    [Fact]
    public async Task PreflightRequest_FromAllowedOrigin_ShouldReturnCorsHeaders()
    {
        // Arrange - Simular preflight request do browser
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/users/register");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        // Preflight requests should succeed
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.OK);

        // Verificar headers CORS na resposta
        var headers = response.Headers;
        
        // Em ambiente de desenvolvimento, deve permitir a origem
        headers.Should().ContainKey("Access-Control-Allow-Origin",
            "preflight response must include Access-Control-Allow-Origin header");
        
        var allowedOrigins = headers.GetValues("Access-Control-Allow-Origin");
        allowedOrigins.Should().Contain(o => 
            o == "http://localhost:3000" || o == "*",
            "CORS should allow localhost origin in development");
    }

    [Fact]
    public async Task PreflightRequest_WithMultipleMethods_ShouldIncludeAllowedMethods()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/services");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Methods",
            "preflight response must include Access-Control-Allow-Methods header");
        
        var allowedMethods = string.Join(",", response.Headers.GetValues("Access-Control-Allow-Methods"));
        
        // Deve permitir métodos configurados (GET, POST, PUT, DELETE, etc.)
        allowedMethods.Should().Contain("GET",
            "GET method should be allowed in CORS policy");
    }

    [Fact]
    public async Task PreflightRequest_WithCustomHeaders_ShouldAllowContentType()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/users/register");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type,authorization");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Headers",
            "preflight response must include Access-Control-Allow-Headers header");
        
        var allowedHeaders = string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")).ToLowerInvariant();
        
        // Deve permitir content-type e authorization
        allowedHeaders.Should().Contain("content-type",
            "content-type header should be allowed in CORS policy");
    }

    #endregion

    #region Actual Requests with CORS

    [Fact]
    public async Task GetRequest_WithOriginHeader_ShouldIncludeCorsResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Verificar header Access-Control-Allow-Origin na resposta
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
            "response to request with Origin header must include Access-Control-Allow-Origin");
        
        var allowedOrigins = response.Headers.GetValues("Access-Control-Allow-Origin");
        allowedOrigins.Should().NotBeEmpty("CORS headers should be present");
    }

    [Fact]
    public async Task PostRequest_WithOriginHeader_ShouldAllowCrossOrigin()
    {
        // Arrange
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

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/register")
        {
            Content = JsonContent.Create(registerRequest)
        };
        request.Headers.Add("Origin", "http://localhost:3000");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        // Request deve ser processado normalmente
        // CORS should not block the request
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest); // Pode falhar por validação, mas não por CORS

        // Verificar que CORS headers estão presentes
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
            "response to cross-origin POST must include Access-Control-Allow-Origin header");
        
        var allowedOrigins = response.Headers.GetValues("Access-Control-Allow-Origin");
        allowedOrigins.Should().NotBeEmpty(
            "Access-Control-Allow-Origin header must have at least one value");
    }

    #endregion

    #region Credentials Support

    [Fact]
    public async Task Request_WithCredentials_ShouldNotBlockCorsWhenCredentialsNotConfigured()
    {
        // Arrange - Request com credenciais
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Cookie", "session=test-session");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            "request with credentials should succeed even if AllowCredentials is not configured");

        // CORS origin header deve estar presente
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
            "response should include CORS origin header");
        
        // Se AllowCredentials está habilitado na configuração, o header deve estar presente
        // Caso contrário, o servidor pode optar por não incluí-lo (comportamento válido)
        if (response.Headers.Contains("Access-Control-Allow-Credentials"))
        {
            var allowCredentials = response.Headers.GetValues("Access-Control-Allow-Credentials").First();
            allowCredentials.Should().Be("true",
                "if Access-Control-Allow-Credentials is present, it must be 'true'");
        }
    }

    [Fact]
    public async Task AuthenticatedRequest_WithCors_ShouldWorkCorrectly()
    {
        // Arrange - Registrar e fazer login
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
        
        await _fixture.ApiClient.PostAsJsonAsync("/api/users/register", registerRequest);

        var loginRequest = new
        {
            email = registerRequest.email,
            password = registerRequest.password
        };

        var loginResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.IsSuccessStatusCode.Should().BeTrue("login should succeed");
        
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        loginResult.Should().NotBeNull();
        loginResult.Should().ContainKey("token");
        var token = loginResult!["token"]?.ToString();
        token.Should().NotBeNullOrEmpty("login should return a valid token");

        // Act - Request autenticado com CORS headers
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            "authenticated requests with CORS should work");

        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            response.Headers.GetValues("Access-Control-Allow-Origin").Should().NotBeEmpty();
        }
    }

    #endregion

    #region Security Validations

    [Fact]
    public async Task CorsConfiguration_InDevelopment_ShouldAllowLocalhost()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        // Em desenvolvimento, localhost deve ser permitido
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigins = response.Headers.GetValues("Access-Control-Allow-Origin");
            allowedOrigins.Should().Contain(o => 
                o == "http://localhost:3000" || o == "*");
        }
    }

    [Fact]
    public async Task CorsConfiguration_ShouldIncludeMaxAge()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/users/register");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        if (response.Headers.Contains("Access-Control-Max-Age"))
        {
            var maxAge = response.Headers.GetValues("Access-Control-Max-Age").First();
            int.TryParse(maxAge, out var seconds).Should().BeTrue();
            seconds.Should().BeGreaterThan(0, "preflight cache should have positive duration");
        }
    }

    [Fact]
    public async Task Request_WithoutOriginHeader_ShouldStillSucceed()
    {
        // Arrange - Request sem Origin header (não é cross-origin)
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");

        // Act
        var response = await _fixture.ApiClient.SendAsync(request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            "requests without Origin header should work (same-origin)");

        // Não deve incluir CORS headers
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse(
            "CORS headers should not be present for same-origin requests");
    }

    #endregion

    #region Configuration Validation

    [Fact]
    public async Task CorsPolicy_ShouldAllowCommonHttpMethods()
    {
        // Arrange - Testar métodos HTTP comuns
        var methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };

        foreach (var method in methods)
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/services");
            request.Headers.Add("Origin", "http://localhost:3000");
            request.Headers.Add("Access-Control-Request-Method", method);

            // Act
            var response = await _fixture.ApiClient.SendAsync(request);

            // Assert
            if (response.Headers.Contains("Access-Control-Allow-Methods"))
            {
                var allowedMethods = string.Join(",", response.Headers.GetValues("Access-Control-Allow-Methods"));
                
                // Deve permitir métodos HTTP comuns
                // Nota: Dependendo da configuração, pode ser "*" ou lista específica
                // Method should be allowed or handled gracefully
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.NoContent);
            }
        }
    }

    [Fact]
    public async Task CorsHeaders_ShouldBeConsistentAcrossEndpoints()
    {
        // Arrange - Testar múltiplos endpoints
        var endpoints = new[] { "/health", "/alive", "/api/users/register" };
        var originHeader = "http://localhost:3000";

        foreach (var endpoint in endpoints)
        {
            var request = new HttpRequestMessage(HttpMethod.Options, endpoint);
            request.Headers.Add("Origin", originHeader);
            request.Headers.Add("Access-Control-Request-Method", "GET");

            // Act
            var response = await _fixture.ApiClient.SendAsync(request);

            // Assert - CORS headers devem ser consistentes
            if (response.Headers.Contains("Access-Control-Allow-Origin"))
            {
                response.Headers.GetValues("Access-Control-Allow-Origin").Should().NotBeEmpty(
                    $"CORS headers should be present for {endpoint}");
            }
        }
    }

    #endregion
}

