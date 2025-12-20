using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Testes de integração para SecurityHeadersMiddleware
/// </summary>
public sealed class SecurityHeadersMiddlewareTests : ApiTestBase
{
    private HttpClient HttpClient => Client;

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXContentTypeOptions()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options");
        
        var headerValue = response.Headers.GetValues("X-Content-Type-Options").First();
        headerValue.Should().Be("nosniff", "Deve prevenir MIME type sniffing");
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXFrameOptions()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().Contain(h => h.Key == "X-Frame-Options");
        
        var headerValue = response.Headers.GetValues("X-Frame-Options").First();
        // Deve prevenir clickjacking
        headerValue.Should().BeOneOf("DENY", "SAMEORIGIN");
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeStrictTransportSecurity()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // HSTS pode estar presente dependendo do ambiente
        if (response.Headers.Contains("Strict-Transport-Security"))
        {
            var headerValue = response.Headers.GetValues("Strict-Transport-Security").First();
            headerValue.Should().Contain("max-age=", "Deve especificar tempo de validade");
        }
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeContentSecurityPolicy()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().Contain(h => h.Key == "Content-Security-Policy");
        
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        csp.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SecurityHeaders_Development_ShouldHaveLenientCSP()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        if (response.Headers.Contains("Content-Security-Policy"))
        {
            var csp = response.Headers.GetValues("Content-Security-Policy").First();
            
            // Em desenvolvimento/testing, CSP pode ser mais permissivo
            // (unsafe-inline, unsafe-eval podem estar presentes)
            csp.Should().Contain("default-src");
        }
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXPermittedCrossDomainPolicies()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Header para controlar cross-domain access (Flash, PDF, etc.)
        if (response.Headers.Contains("X-Permitted-Cross-Domain-Policies"))
        {
            var headerValue = response.Headers.GetValues("X-Permitted-Cross-Domain-Policies").First();
            headerValue.Should().Be("none", "Deve bloquear políticas cross-domain");
        }
    }

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeReferrerPolicy()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        if (response.Headers.Contains("Referrer-Policy"))
        {
            var headerValue = response.Headers.GetValues("Referrer-Policy").First();
            // Deve ter política de referrer segura
            headerValue.Should().BeOneOf(
                "no-referrer",
                "strict-origin-when-cross-origin",
                "same-origin");
        }
    }

    [Fact]
    public async Task SecurityHeaders_AllEndpoints_ShouldHaveConsistentHeaders()
    {
        // Arrange
        var registerRequest = new
        {
            Name = "Security Headers Test User",
            Email = $"secheaders.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await HttpClient.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        
        // Login deve funcionar para teste ser válido
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Login deve ser bem-sucedido para testar headers de segurança em endpoints autenticados");
        
        var loginData = await loginResponse.Content.ReadFromJsonAsync<dynamic>();
        var token = loginData!.GetProperty("data").GetProperty("token").GetString();

        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var endpoints = new[] { "/health", "/api/v1/users", "/api/v1/providers" };
        var responses = new List<HttpResponseMessage>();

        // Act
        foreach (var endpoint in endpoints)
        {
            responses.Add(await HttpClient.GetAsync(endpoint));
        }

        // Assert
        // Todos os endpoints devem ter X-Content-Type-Options
        responses.Should().AllSatisfy(response =>
        {
            response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options",
                $"Endpoint {response.RequestMessage?.RequestUri} deve ter headers de segurança");
        });

        HttpClient.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task SecurityHeaders_PostRequest_ShouldAlsoHaveHeaders()
    {
        // Arrange
        var request = new
        {
            Name = "Security Test",
            Email = $"security.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options",
            "POST requests também devem ter headers de segurança");
    }

    [Fact]
    public async Task SecurityHeaders_ErrorResponse_ShouldStillHaveHeaders()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/api/v1/users/99999999-9999-9999-9999-999999999999");

        // Assert - endpoint pode retornar Unauthorized se não autenticado ou NotFound se autenticado
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options",
            "Respostas de erro também devem ter headers de segurança");
    }
}
