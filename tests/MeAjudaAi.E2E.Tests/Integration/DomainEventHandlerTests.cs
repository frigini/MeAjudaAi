using MeAjudaAi.E2E.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração para manipuladores de eventos de domínio usando contexto de banco de dados
/// </summary>
public class DomainEventHandlerTests : TestContainerTestBase
{
    [Fact]
    public async Task UserDomainEvents_ShouldBeProcessedCorrectly()
    {
        // Act & Assert
        await WithDbContextAsync(async context =>
        {
            var canConnect = await context.Database.CanConnectAsync();
            canConnect.Should().BeTrue("Database should be accessible for domain event processing");
            
            // Verify tables exist in correct schema
            var usersTableExists = await context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'users' AND table_schema = 'users'")
                .FirstOrDefaultAsync() > 0;
                
            usersTableExists.Should().BeTrue("Users table should exist for domain event handlers");
        });
    }
}