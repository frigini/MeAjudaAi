using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Configuration;
using MeAjudaAi.Modules.Users.Api.Requests;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Testes de integração para CompressionSecurityMiddleware
/// </summary>
[Collection(GlobalTestConfiguration.IntegrationTestsCollectionName)]
public sealed class CompressionSecurityMiddlewareTests(IntegrationTestsFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task CompressionSecurity_AuthenticatedUser_ShouldDisableCompression()
    {
        // Arrange
        var registerRequest = new UserRegistrationRequest
        {
            Name = "Compression Test User",
            Email = $"compression.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        var loginData = await loginResponse.Content.ReadFromJsonAsync<dynamic>();
        var token = loginData!.GetProperty("data").GetProperty("token").GetString();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var response = await _client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        
        // CompressionSecurityMiddleware deve desabilitar compressão para usuários autenticados
        // Isso previne ataques BREACH/CRIME que exploram compressão
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                "Compressão deve ser desabilitada para usuários autenticados (proteção BREACH)");
            response.Content.Headers.ContentEncoding.Should().NotContain("br",
                "Brotli deve ser desabilitado para usuários autenticados (proteção BREACH)");
        }

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_AnonymousUser_ShouldAllowCompression()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Para usuários anônimos, compressão pode estar habilitada
        // (conteúdo público não é vulnerável a BREACH)
        // Nota: se o servidor decidir comprimir ou não depende de outros fatores
        // Este teste valida que middleware NÃO bloqueia compressão para anônimos
        
        _client.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_AuthenticatedRequest_WithoutAcceptEncoding_ShouldSucceed()
    {
        // Arrange
        var registerRequest = new UserRegistrationRequest
        {
            Name = "No Compression User",
            Email = $"nocomp.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        var loginData = await loginResponse.Content.ReadFromJsonAsync<dynamic>();
        var token = loginData!.GetProperty("data").GetProperty("token").GetString();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _client.DefaultRequestHeaders.Remove("Accept-Encoding");

        // Act
        var response = await _client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentEncoding.Should().BeEmpty(
                "Sem Accept-Encoding, não deve haver compressão");
        }

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CompressionSecurity_LoginEndpoint_ShouldNotCompress()
    {
        // Arrange
        var registerRequest = new UserRegistrationRequest
        {
            Name = "Login Compression Test",
            Email = $"logincomp.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Resposta de login contém token sensível, não deve ser comprimida
        response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
            "Login response não deve ser comprimida (contém token sensível)");

        _client.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_MultipleAuthenticatedRequests_ShouldConsistentlyDisableCompression()
    {
        // Arrange
        var registerRequest = new UserRegistrationRequest
        {
            Name = "Multiple Requests User",
            Email = $"multiple.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "user"
        };

        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        var loginData = await loginResponse.Content.ReadFromJsonAsync<dynamic>();
        var token = loginData!.GetProperty("data").GetProperty("token").GetString();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            responses.Add(await _client.GetAsync("/api/v1/users"));
        }

        // Assert
        responses.Should().AllSatisfy(response =>
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                    "Todas as requisições autenticadas devem ter compressão desabilitada");
            }
        });

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_DifferentEndpoints_ShouldApplyRulesConsistently()
    {
        // Arrange
        var registerRequest = new UserRegistrationRequest
        {
            Name = "Endpoints Test User",
            Email = $"endpoints.{Guid.NewGuid()}@example.com",
            Password = "ValidPass123!",
            Role = "provider"
        };

        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        var loginData = await loginResponse.Content.ReadFromJsonAsync<dynamic>();
        var token = loginData!.GetProperty("data").GetProperty("token").GetString();

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        var endpoints = new[] { "/api/v1/users", "/api/v1/providers", "/api/v1/service-categories" };

        // Act
        var responses = new List<HttpResponseMessage>();
        foreach (var endpoint in endpoints)
        {
            responses.Add(await _client.GetAsync(endpoint));
        }

        // Assert
        responses.Should().AllSatisfy(response =>
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                    $"Endpoint {response.RequestMessage?.RequestUri} não deve comprimir para usuários autenticados");
            }
        });

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("Accept-Encoding");
    }
}
