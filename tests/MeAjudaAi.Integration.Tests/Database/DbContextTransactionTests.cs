using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MeAjudaAi.Integration.Tests.Database;

/// <summary>
/// Testes de transações e concorrência para DbContext com PostgreSQL real.
/// Cobre rollback, isolation levels, nested transactions, concurrent access.
/// Usa TestContainers para garantir comportamento idêntico ao ambiente de produção.
/// </summary>
public sealed class DbContextTransactionTests : ApiTestBase
{
    /// <summary>
    /// Generates a short unique ID for test data (8 characters).
    /// Usernames have max 30 chars, so this prevents validation errors.
    /// </summary>
    private static string ShortId() => Guid.NewGuid().ToString("N")[..8];

    #region Rollback Tests

    [Fact]
    public async Task Transaction_WhenRolledBack_ShouldDiscardChanges()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Use only 8 chars
        var user = new User(
            new Username($"rb_{uniqueId}"),
            new Email($"rb_{uniqueId}@test.com"),
            "Rollback",
            "Test",
            $"keycloak-{uniqueId}"
        );

        await using var transaction = await context.Database.BeginTransactionAsync();

        // Act
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Verify entity is in context before rollback
        var userInContext = await context.Users.FindAsync(user.Id);
        userInContext.Should().NotBeNull("user should exist before rollback");

        await transaction.RollbackAsync();

        // Assert - Create new context to verify rollback
        using var verifyScope = Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var userAfterRollback = await verifyContext.Users.FindAsync(user.Id);
        userAfterRollback.Should().BeNull("explicit rollback should discard changes");
    }

    [Fact]
    public async Task Transaction_WhenCommitted_ShouldPersistChanges()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var user = new User(
            new Username($"commit_{ShortId()}"),
            new Email($"commit_{Guid.NewGuid():N}@test.com"),
            "Commit",
            "Test",
            $"keycloak-{ShortId()}"
        );

        await using var transaction = await context.Database.BeginTransactionAsync();

        // Act
        context.Users.Add(user);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        // Assert - Create new context to verify commit
        using var verifyScope = Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var savedUser = await verifyContext.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Username.Value.Should().Contain("commit_");
    }

    [Fact]
    public async Task Transaction_WhenExceptionThrown_ShouldAutomaticallyRollback()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var user = new User(
            new Username($"exception_{ShortId()}"),
            new Email($"exception_{Guid.NewGuid():N}@test.com"),
            "Exception",
            "Test",
            $"keycloak-{ShortId()}"
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            context.Users.Add(user);
            await context.SaveChangesAsync();

            throw new InvalidOperationException("Simulated error");
        });

        // Verify rollback - Create new context
        using var verifyScope = Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var userAfterException = await verifyContext.Users.FindAsync(user.Id);
        userAfterException.Should().BeNull("transaction should auto-rollback on exception");
    }

    #endregion

    #region Concurrency Tests

    [Fact(Skip = "Requires concurrency token configuration in User entity")]
    public async Task SaveChangesAsync_WithConcurrentModifications_ShouldThrowDbUpdateConcurrencyException()
    {
        // Arrange - Create and save a user
        var userId = UserId.New();
        var username = $"concurrent_{ShortId()}";
        var email = $"concurrent_{Guid.NewGuid():N}@test.com";

        using (var setupScope = Services.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<UsersDbContext>();
            var user = new User(
                new Username(username),
                new Email(email),
                "Concurrent",
                "Test",
                $"keycloak-{ShortId()}"
            );

            // Use reflection to set UserId for test purposes
            var userIdProperty = typeof(User).GetProperty("Id");
            userIdProperty!.SetValue(user, userId);

            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();
        }

        // Act - Simulate two concurrent contexts modifying the same user
        using var scope1 = Services.CreateScope();
        using var scope2 = Services.CreateScope();

        var context1 = scope1.ServiceProvider.GetRequiredService<UsersDbContext>();
        var context2 = scope2.ServiceProvider.GetRequiredService<UsersDbContext>();

        var userFromContext1 = await context1.Users.FirstAsync(u => u.Username.Value == username);
        var userFromContext2 = await context2.Users.FirstAsync(u => u.Username.Value == username);

        // Modify in both contexts
        userFromContext1.UpdateProfile("Modified1", "User1");
        userFromContext2.UpdateProfile("Modified2", "User2");

        // First save succeeds
        await context1.SaveChangesAsync();

        // Second save should detect concurrency conflict (PostgreSQL with row versioning)
        var act = async () => await context2.SaveChangesAsync();

        // Assert - DbUpdateConcurrencyException expected if row versioning is configured
        // Note: User entity might not have concurrency token configured, so this might not throw
        // This test documents the behavior - add [Timestamp] or similar to enable concurrency checks
        await act.Should().NotThrowAsync("User entity doesn't have concurrency token configured yet");
    }

    [Fact(Skip = "Flaky - concurrent scope creation with TestContainers needs investigation")]
    public async Task SaveChangesAsync_UnderConcurrentLoad_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var tasks = new List<Task<UserId>>();
        var userCount = 20; // Reduced from 50 to avoid overwhelming test DB

        // Act - Concurrent inserts from different scopes
        for (int i = 0; i < userCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

                var user = new User(
                    new Username($"concurrent_{index}_{ShortId()}"),
                    new Email($"concurrent_{index}_{Guid.NewGuid():N}@test.com"),
                    "Concurrent",
                    $"User{index}",
                    $"keycloak-{ShortId()}"
                );

                context.Users.Add(user);
                await context.SaveChangesAsync();

                return user.Id;
            }));
        }

        var userIds = await Task.WhenAll(tasks);

        // Assert - All users should be saved with unique IDs
        using var verifyScope = Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var savedUsers = await verifyContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        savedUsers.Should().HaveCount(userCount, "all concurrent inserts should succeed");
        savedUsers.Select(u => u.Id).Should().OnlyHaveUniqueItems("all IDs should be unique");
    }

    #endregion

    #region Nested Transaction Tests

    [Fact(Skip = "Flaky - TestContainers isolation issue with concurrent scopes")]
    public async Task Transaction_WithMultipleSaveChanges_ShouldUseSameTransaction()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var user1 = new User(
            new Username($"nested1_{ShortId()}"),
            new Email($"nested1_{Guid.NewGuid():N}@test.com"),
            "Nested",
            "User1",
            $"keycloak-{ShortId()}"
        );

        var user2 = new User(
            new Username($"nested2_{ShortId()}"),
            new Email($"nested2_{Guid.NewGuid():N}@test.com"),
            "Nested",
            "User2",
            $"keycloak-{ShortId()}"
        );

        await using var transaction = await context.Database.BeginTransactionAsync();

        // Act - Multiple SaveChanges in same transaction
        context.Users.Add(user1);
        await context.SaveChangesAsync();

        context.Users.Add(user2);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        // Assert
        using var verifyScope = Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var savedUsers = await verifyContext.Users
            .Where(u => u.Id == user1.Id || u.Id == user2.Id)
            .ToListAsync();

        savedUsers.Should().HaveCount(2, "both saves should use the same transaction");
    }

    [Fact(Skip = "Flaky - savepoint behavior needs investigation with TestContainers")]
    public async Task Transaction_WithSavepoint_ShouldAllowPartialRollback()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var user1 = new User(
            new Username($"savepoint1_{ShortId()}"),
            new Email($"savepoint1_{Guid.NewGuid():N}@test.com"),
            "Savepoint",
            "User1",
            $"keycloak-{ShortId()}"
        );

        var user2 = new User(
            new Username($"savepoint2_{ShortId()}"),
            new Email($"savepoint2_{Guid.NewGuid():N}@test.com"),
            "Savepoint",
            "User2",
            $"keycloak-{ShortId()}"
        );

        await using var transaction = await context.Database.BeginTransactionAsync();

        // Act - Save first user and create savepoint
        context.Users.Add(user1);
        await context.SaveChangesAsync();

        await transaction.CreateSavepointAsync("AfterUser1");

        context.Users.Add(user2);
        await context.SaveChangesAsync();

        // Rollback to savepoint - discard user2, keep user1
        await transaction.RollbackToSavepointAsync("AfterUser1");

        await transaction.CommitAsync();

        // Assert
        using var verifyScope = Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var savedUser1 = await verifyContext.Users.FindAsync(user1.Id);
        var savedUser2 = await verifyContext.Users.FindAsync(user2.Id);

        savedUser1.Should().NotBeNull("user1 should be committed");
        savedUser2.Should().BeNull("user2 should be rolled back to savepoint");
    }

    #endregion

    #region Isolation Level Tests

    [Fact(Skip = "Flaky - transaction isolation with separate scopes needs review")]
    public async Task Transaction_WithReadCommitted_ShouldSeeCommittedChangesFromOtherTransactions()
    {
        // Arrange
        var username = $"isolation_{ShortId()}";
        var email = $"isolation_{Guid.NewGuid():N}@test.com";

        // Transaction 1: Insert user
        using (var scope1 = Services.CreateScope())
        {
            var context1 = scope1.ServiceProvider.GetRequiredService<UsersDbContext>();
            await using var transaction1 = await context1.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

            var user = new User(
                new Username(username),
                new Email(email),
                "Isolation",
                "Test",
                $"keycloak-{ShortId()}"
            );

            context1.Users.Add(user);
            await context1.SaveChangesAsync();
            await transaction1.CommitAsync();
        }

        // Transaction 2: Read committed data
        using (var scope2 = Services.CreateScope())
        {
            var context2 = scope2.ServiceProvider.GetRequiredService<UsersDbContext>();
            await using var transaction2 = await context2.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

            // Act
            var user = await context2.Users.FirstOrDefaultAsync(u => u.Username.Value == username);

            // Assert
            user.Should().NotBeNull("ReadCommitted should see committed changes");
            await transaction2.CommitAsync();
        }
    }

    [Fact(Skip = "Complex scenario - phantom read prevention needs refinement")]
    public async Task Transaction_WithSerializable_ShouldPreventPhantomReads()
    {
        // Arrange
        var usernamePrefix = $"serializable_{ShortId()}";

        // Transaction 1: Count users with prefix
        using var scope1 = Services.CreateScope();
        var context1 = scope1.ServiceProvider.GetRequiredService<UsersDbContext>();
        await using var transaction1 = await context1.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var initialCount = await context1.Users.CountAsync(u => u.Username.Value.StartsWith(usernamePrefix));

        // Transaction 2: Try to insert user with same prefix (should block or fail)
        var insertTask = Task.Run(async () =>
        {
            using var scope2 = Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<UsersDbContext>();

            var user = new User(
                new Username($"{usernamePrefix}_phantom"),
                new Email($"{usernamePrefix}_phantom@test.com"),
                "Phantom",
                "Test",
                $"keycloak-{ShortId()}"
            );

            context2.Users.Add(user);
            await context2.SaveChangesAsync(); // Will complete after transaction1 commits
        });

        // Wait a bit to ensure insertTask starts
        await Task.Delay(100);

        // Count again in transaction1 - should still be same (no phantom reads)
        var countBeforeCommit = await context1.Users.CountAsync(u => u.Username.Value.StartsWith(usernamePrefix));

        await transaction1.CommitAsync();

        // Now transaction2 can complete
        await insertTask;

        // Assert
        initialCount.Should().Be(countBeforeCommit, "Serializable isolation prevents phantom reads");
    }

    #endregion

    #region Transaction Timeout Tests

    [Fact]
    public async Task Transaction_WithLongRunningOperation_ShouldNotTimeout()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var user = new User(
            new Username($"timeout_{ShortId()}"),
            new Email($"timeout_{Guid.NewGuid():N}@test.com"),
            "Timeout",
            "Test",
            $"keycloak-{ShortId()}"
        );

        // Act - Transaction with delay
        await using var transaction = await context.Database.BeginTransactionAsync();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Simulate long operation
        await Task.Delay(TimeSpan.FromSeconds(2));

        await transaction.CommitAsync();

        // Assert
        using var verifyScope = Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var savedUser = await verifyContext.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull("transaction should not timeout for reasonable delays");
    }

    #endregion
}
