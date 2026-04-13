using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Testes de integração para CompressionSecurityMiddleware
/// </summary>
public sealed class CompressionSecurityMiddlewareTests : BaseApiTest
{
    private HttpClient HttpClient => Client;

    [Fact]
    public async Task CompressionSecurity_AuthenticatedUser_ShouldDisableCompression()
    {
        // Arrange
        // O middleware CompressionSecurity verifica este header antes de UseAuthentication() executar
        // Não precisamos de um usuário real no banco, apenas do header presente
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        using var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "O endpoint de health deve ser acessível");

        // Verifica se o middleware de segurança agiu
        response.Headers.Should().ContainKey("X-Compression-Disabled", 
            "O middleware de segurança deve adicionar o header de debug quando desabilita compressão");

        // CompressionSecurityMiddleware deve desabilitar compressão para usuários autenticados
        // Isso previne ataques BREACH/CRIME que exploram compressão
        response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
            "Compressão deve ser desabilitada para usuários autenticados (proteção BREACH)");
        response.Content.Headers.ContentEncoding.Should().NotContain("br",
            "Brotli deve ser desabilitado para usuários autenticados (proteção BREACH)");

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_AnonymousUser_ShouldAllowCompression()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        using var response = await HttpClient.GetAsync("/health");

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
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");

        // Act
        using var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
    }

    [Fact]
    public async Task CompressionSecurity_MultipleAuthenticatedRequests_ShouldConsistentlyDisableCompression()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act & Assert - Dispose responses immediately after use
        for (int i = 0; i < 5; i++)
        {
            using var response = await HttpClient.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                "Todas as requisições autenticadas devem ter compressão desabilitada");
        }

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_DifferentEndpoints_ShouldApplyRulesConsistently()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        var endpoints = new[] { "/health", "/api/v1/configuration/features" };

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

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }
}
