using System.Net;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes E2E para RateLimitingMiddleware
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Infrastructure")]
public sealed class RateLimitingEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public RateLimitingEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RateLimiting_ManyRequests_ShouldProcessCorrectly()
    {
        // Arrange
        _fixture.ApiClient.DefaultRequestHeaders.Remove("Authorization");
        const int requests = 50;

        // Act
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < requests; i++)
        {
            responses.Add(await _fixture.ApiClient.GetAsync("/health"));
        }

        // Assert
        // Verificar que todas as requisições foram processadas
        responses.Should().AllSatisfy(response =>
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.TooManyRequests);
        });

        // Pelo menos algumas devem ter sucesso
        var successfulRequests = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        successfulRequests.Should().BeGreaterThan(0);

        // Se houver bloqueio por rate limiting, verificar header
        var blockedResponse = responses.FirstOrDefault(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        if (blockedResponse != null)
        {
            blockedResponse.Headers.Should().ContainKey("Retry-After", 
                "Header Retry-After deve estar presente em 429");
        }
    }

    [Fact]
    public async Task RateLimiting_DifferentEndpoints_ShouldProcess()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();

        // Act
        var healthResponses = new List<HttpResponseMessage>();
        var apiResponses = new List<HttpResponseMessage>();

        for (int i = 0; i < 30; i++)
        {
            healthResponses.Add(await _fixture.ApiClient.GetAsync("/health"));
            apiResponses.Add(await _fixture.ApiClient.GetAsync("/api/v1/service-categories"));
        }

        // Assert
        // Verificar se há rate limiting configurado
        var healthBlocked = healthResponses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var apiBlocked = apiResponses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        // Pelo menos um endpoint não deve estar totalmente bloqueado
        (healthBlocked < 30 || apiBlocked < 30).Should().BeTrue(
            "Rate limiting deve permitir algumas requisições");
    }

    [Fact]
    public async Task RateLimiting_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        _fixture.ApiClient.DefaultRequestHeaders.Remove("Authorization");
        const int concurrentRequests = 30;

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => _fixture.ApiClient.GetAsync("/health"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert
        // Todas as requisições devem ser processadas
        responses.Should().AllSatisfy(response =>
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.TooManyRequests);
        });

        // Pelo menos algumas devem ter sucesso
        var successfulRequests = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        successfulRequests.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RateLimiting_AuthenticatedUser_ShouldProcess()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsUser();

        // Act
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 50; i++)
        {
            responses.Add(await _fixture.ApiClient.GetAsync("/api/v1/service-categories"));
        }

        // Assert
        // Usuários autenticados devem poder fazer requisições
        var successful = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var blocked = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        successful.Should().BeGreaterThan(0, "Usuários autenticados devem ter acesso");
        
        // Se houver bloqueio, deve ser menos severo que para anônimos
        if (blocked > 0)
        {
            successful.Should().BeGreaterThan(blocked, 
                "Usuários autenticados devem ter limite maior");
        }
    }
}

