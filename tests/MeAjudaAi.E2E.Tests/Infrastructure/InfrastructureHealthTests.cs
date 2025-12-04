using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes de saúde da infraestrutura TestContainers.
/// Valida se PostgreSQL e Redis estão funcionando corretamente.
/// NOTE: API health check test removed - duplicates HealthCheckTests which is more comprehensive
/// 
/// MIGRADO PARA IClassFixture: Compartilha containers entre todos os testes desta classe.
/// Reduz overhead de ~18s (3 testes × 6s) para ~6s (1× setup).
/// </summary>
public class InfrastructureHealthTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;
    private readonly HttpClient _apiClient;

    public InfrastructureHealthTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
        _apiClient = fixture.ApiClient;
    }

    // NOTE: Api_Should_Respond_To_Health_Check removed - duplicates HealthCheckTests.HealthCheck_ShouldReturnHealthy
    // HealthCheckTests is more comprehensive (tests /health, /health/live, /health/ready)

    [Fact]
    public async Task Database_Should_Be_Available_And_Migrated()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Act
        var canConnect = await dbContext.Database.CanConnectAsync();
        var usersCount = await dbContext.Users.CountAsync();

        // Assert
        canConnect.Should().BeTrue("Database should be reachable");
        usersCount.Should().BeGreaterThanOrEqualTo(0, "Users table should exist");
    }

    [Fact]
    public async Task Redis_Should_Be_Available()
    {
        // Este teste verifica indiretamente se o Redis está funcionando
        // A API deve conseguir inicializar com o Redis configurado

        // Act
        var response = await _apiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "API should start successfully with Redis configured");
    }
}

