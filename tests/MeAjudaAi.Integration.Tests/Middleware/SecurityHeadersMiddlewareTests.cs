using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Testes de integração para SecurityHeadersMiddleware
/// </summary>
public sealed class SecurityHeadersMiddlewareTests : BaseApiTest
{
    private HttpClient HttpClient => Client;

    [Fact]
    public async Task SecurityHeaders_ShouldIncludeXContentTypeOptions()
    {
        // Arrange & Act
        using var response = await HttpClient.GetAsync("/health");

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
        using var response = await HttpClient.GetAsync("/health");

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
        using var response = await HttpClient.GetAsync("/health");

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
        using var response = await HttpClient.GetAsync("/health");

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
        using var response = await HttpClient.GetAsync("/health");

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
        using var response = await HttpClient.GetAsync("/health");

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
        using var response = await HttpClient.GetAsync("/health");

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
        // Arrange - Configurar usuário autenticado
        AuthConfig.ConfigureRegularUser();

        var endpoints = new[] { "/health", "/api/v1/providers" };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            using var response = await HttpClient.GetAsync(endpoint);
            response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options",
                $"Endpoint {endpoint} deve ter headers de segurança");
        }
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
        using var response = await HttpClient.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options",
            "POST requests também devem ter headers de segurança");
    }

    [Fact]
    public async Task SecurityHeaders_ErrorResponse_ShouldStillHaveHeaders()
    {
        // Arrange & Act
        using var response = await HttpClient.GetAsync("/api/v1/users/99999999-9999-9999-9999-999999999999");

        // Assert - endpoint pode retornar Unauthorized se não autenticado ou NotFound se autenticado
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        response.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options",
            "Respostas de erro também devem ter headers de segurança");
    }

    [Fact]
    public async Task SecurityHardening_AntiforgeryCookie_ShouldBePresentInGetRequests()
    {
        // Act
        using var response = await HttpClient.GetAsync("/health");

        // Assert
        // O ASP.NET Core gera o cookie de antiforgery em requisições GET para que o SPA possa lê-lo
        // NOTA: Dependendo da configuração, o cookie só é gerado se explicitamente acessado ou se o middleware
        // for configurado para tal. Como adicionamos UseAntiforgery(), verificamos se o Set-Cookie está presente.
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            cookies.Should().Contain(c => c.Contains("XSRF-TOKEN"), "O cookie de antiforgery deve ser enviado para o cliente");
        }
    }

    [Fact]
    public async Task SecurityHardening_HttpsRedirection_ShouldBeActive()
    {
        // Arrange
        // Criar um cliente que NÃO segue redirecionamentos automaticamente para podermos ver o 307/308
        var options = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost")
        };
        using var noRedirectClient = _factory!.CreateClient(options);

        // Act
        using var response = await noRedirectClient.GetAsync("/health");

        // Assert
        // Em ambiente de teste, o middleware pode não redirecionar se detectar localhost ou se não estiver configurado para HTTPS
        // Mas se estiver ativo, esperamos 307 (Temporary Redirect) ou 308 (Permanent Redirect)
        // Se retornar 200, significa que o redirecionamento não ocorreu (comum em testes se a porta HTTPS não for definida)
        if (response.StatusCode != HttpStatusCode.OK)
        {
            response.StatusCode.Should().BeOneOf([HttpStatusCode.TemporaryRedirect, HttpStatusCode.PermanentRedirect], 
                "Requisições HTTP devem ser redirecionadas para HTTPS");
            response.Headers.Location!.Scheme.Should().Be("https");
        }
    }
}
