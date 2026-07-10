using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes de saúde da infraestrutura TestContainers.
/// Valida disponibilidade do PostgreSQL.
/// Redis e demais dependências são cobertos por HealthCheckTests.
/// </summary>
/// <remarks>
/// MIGRADO PARA IClassFixture: Compartilha containers entre todos os testes desta classe.
/// Reduz overhead de ~18s (3 testes × 6s) para ~6s (1× setup).
/// </remarks>
public class InfrastructureHealthTests(TestContainerFixture fixture) : BaseE2ETest<TestContainerFixture>(fixture)
{
    [Fact]
    public async Task Database_Should_Be_Available_And_Migrated()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Act
        var canConnect = await dbContext.Database.CanConnectAsync();
        var usersCount = await dbContext.Users.CountAsync();

        // Assert
        canConnect.Should().BeTrue("Database should be reachable");
        usersCount.Should().BeGreaterThanOrEqualTo(0, "Users table should exist");
    }
}