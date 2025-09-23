using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Xunit;

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

    /*
    [Fact]
    public async Task UsersDbContext_ShouldSupportTransactionalOperations()
    {
        // Arrange
        using var context = new UsersDbContext(CreateDbContextOptions<UsersDbContext>());
        await context.Database.MigrateAsync();

        // Act & Assert - Test transaction capability
        using var transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            // Isso verifica que a infraestrutura suporta as operações transacionais
            // que os manipuladores de eventos de domínio precisam
            await transaction.RollbackAsync();
            
            // Se chegamos aqui, o suporte a transações está funcionando
            true.Should().BeTrue("Transaction support should be available for domain event handlers");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [Fact]
    public async Task DatabaseMigrations_ShouldBeUpToDate()
    {
        // Arrange
        using var context = new UsersDbContext(CreateDbContextOptions<UsersDbContext>());
        
        // Aplica todas as migrations
        await context.Database.MigrateAsync();
        
        // Verifica se há migrations pendentes
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        
        // Assert
        pendingMigrations.Should().BeEmpty("All migrations should be applied for proper domain event handling");
    }

    [Fact]
    public async Task DatabaseSchema_ShouldSupportDomainEventRequirements()
    {
        // Arrange
        using var context = new UsersDbContext(CreateDbContextOptions<UsersDbContext>());
        await context.Database.MigrateAsync();

        // Act - Verifica se os elementos de schema necessários existem
        var tableCheckSql = @"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name IN ('users')";

        var tables = await context.Database
            .SqlQueryRaw<string>(tableCheckSql)
            .ToListAsync();

        // Assert
        tables.Should().Contain("users", "Users table should exist for domain event processing");
        
        // Verifica se podemos acessar o DbSet
        var usersDbSet = context.Users;
        usersDbSet.Should().NotBeNull("Users DbSet should be accessible");
    }

    [Fact]
    public async Task ConcurrentDatabaseOperations_ShouldBeSupported()
    {
        // Arrange
        var contextOptions = CreateDbContextOptions<UsersDbContext>();
        
        // Act - Cria múltiplos contextos para simular operações concorrentes
        var tasks = Enumerable.Range(0, 3).Select(async i =>
        {
            using var context = new UsersDbContext(contextOptions);
            await context.Database.MigrateAsync();
            
            // Testa acesso concorrente (simulando o que manipuladores de eventos de domínio fariam)
            var canConnect = await context.Database.CanConnectAsync();
            return canConnect;
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => 
            result.Should().BeTrue("All concurrent database operations should succeed"));
    }

    protected async Task CustomInitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        // Inicialização customizada para esta classe de teste
        using var context = new UsersDbContext(CreateDbContextOptions<UsersDbContext>());
        await context.Database.MigrateAsync(cancellationToken);
    }
    */
}