using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes de saúde da infraestrutura TestContainers
/// Valida se PostgreSQL, Redis e API estão funcionando corretamente
/// </summary>
public class InfrastructureHealthTests : TestContainerTestBase
{
    [Fact]
    public async Task Api_Should_Respond_To_Health_Check()
    {
        // Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Database_Should_Be_Available_And_Migrated()
    {
        // Act & Assert
        await WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<UsersDbContext>();

            // Verificar se consegue se conectar ao banco
            var canConnect = await dbContext.Database.CanConnectAsync();
            canConnect.Should().BeTrue("Database should be reachable");

            // Verificar se a tabela users existe testando uma query simples
            var usersCount = await dbContext.Users.CountAsync();
            // Se chegou até aqui sem erro, a tabela existe e está funcionando
            usersCount.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public async Task Redis_Should_Be_Available()
    {
        // Este teste verifica indiretamente se o Redis está funcionando
        // A API deve conseguir inicializar com o Redis configurado

        // Act
        var response = await ApiClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "API should start successfully with Redis configured");
    }
}
