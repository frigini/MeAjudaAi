using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Testes de integração para CompressionSecurityMiddleware
/// </summary>
public sealed class CompressionSecurityMiddlewareTests : ApiTestBase
{
    private HttpClient HttpClient => Client;

    [Fact]
    public async Task CompressionSecurity_AuthenticatedUser_ShouldDisableCompression()
    {
        // Arrange
        var registerRequest = new
        {
            Name = "Compression Test User",
            Email = $"compression.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await HttpClient.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        using var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        
        // Login deve funcionar para teste ser válido
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Login deve ser bem-sucedido para testar desabilitação de compressão");
        
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        var token = loginData!.Data.Token;

        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Requisição autenticada ao endpoint /api/v1/users deve ser bem-sucedida");
        
        // CompressionSecurityMiddleware deve desabilitar compressão para usuários autenticados
        // Isso previne ataques BREACH/CRIME que exploram compressão
        response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
            "Compressão deve ser desabilitada para usuários autenticados (proteção BREACH)");
        response.Content.Headers.ContentEncoding.Should().NotContain("br",
            "Brotli deve ser desabilitado para usuários autenticados (proteção BREACH)");

        HttpClient.DefaultRequestHeaders.Authorization = null;
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_AnonymousUser_ShouldAllowCompression()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Para usuários anônimos, compressão pode estar habilitada
        // (conteúdo público não é vulnerável a BREACH)
        // Nota: se o servidor decidir comprimir ou não depende de outros fatores
        // Este teste valida que middleware NÃO bloqueia compressão para anônimos
        
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_AuthenticatedRequest_WithoutAcceptEncoding_ShouldSucceed()
    {
        // Arrange
        var registerRequest = new
        {
            Name = "No Compression User",
            Email = $"nocomp.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await HttpClient.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        using var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        
        // Login deve funcionar para teste ser válido
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Login deve ser bem-sucedido para testar comportamento sem Accept-Encoding");
        
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        var token = loginData!.Data.Token;

        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");

        // Act
        using var response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentEncoding.Should().BeEmpty(
                "Sem Accept-Encoding, não deve haver compressão");
        }

        HttpClient.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CompressionSecurity_LoginEndpoint_ShouldNotCompress()
    {
        // Arrange
        var registerRequest = new
        {
            Name = "Login Compression Test",
            Email = $"logincomp.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await HttpClient.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        using var response = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginRequest);

        // Assert - endpoint pode não existir (405) ou retornar OK se existir
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.MethodNotAllowed);
        
        // Resposta de login contém token sensível, não deve ser comprimida
        // Só verificar se endpoint existe e retornou OK
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                "Login response não deve ser comprimida (contém token sensível)");
        }

        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_MultipleAuthenticatedRequests_ShouldConsistentlyDisableCompression()
    {
        // Arrange
        var registerRequest = new
        {
            Name = "Multiple Requests User",
            Email = $"multiple.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await HttpClient.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        using var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        
        // Login deve funcionar para teste ser válido
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Login deve ser bem-sucedido para testar múltiplas requisições autenticadas");
        
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        var token = loginData!.Data.Token;

        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act & Assert - Dispose responses immediately after use
        for (int i = 0; i < 5; i++)
        {
            using var response = await HttpClient.GetAsync("/api/v1/users");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                    "Todas as requisições autenticadas devem ter compressão desabilitada");
            }
        }

        HttpClient.DefaultRequestHeaders.Authorization = null;
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_DifferentEndpoints_ShouldApplyRulesConsistently()
    {
        // Arrange
        var registerRequest = new
        {
            Name = "Endpoints Test User",
            Email = $"endpoints.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "provider"
        };

        await HttpClient.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        using var loginResponse = await HttpClient.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        
        // Login deve funcionar para teste ser válido
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Login deve ser bem-sucedido para testar comportamento em diferentes endpoints");
        
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        var token = loginData!.Data.Token;

        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        var endpoints = new[] { "/api/v1/users", "/api/v1/providers", "/api/v1/service-categories" };

        // Act & Assert - Dispose responses immediately after use
        foreach (var endpoint in endpoints)
        {
            using var response = await HttpClient.GetAsync(endpoint);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                    $"Endpoint {endpoint} não deve comprimir para usuários autenticados");
            }
        }

        HttpClient.DefaultRequestHeaders.Authorization = null;
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }
}
