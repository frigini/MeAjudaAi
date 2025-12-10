using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Integration.Tests.Database;

/// <summary>
/// Testes de concorrência otimista usando RowVersion (PostgreSQL xmin).
/// Valida detecção de conflitos em modificações concorrentes.
/// </summary>
public sealed class DbContextConcurrencyTests : ApiTestBase
{
    [Fact]
    public async Task SaveChangesAsync_WithConcurrentModifications_ShouldThrowConcurrencyException()
    {
        // Arrange - Create and save a user
        var username = $"concurrent_{Guid.NewGuid():N}"[..20];
        var email = $"{username}@test.com";

        Guid userId;
        using (var setupScope = Services.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<UsersDbContext>();
            var user = new User(
                new Username(username),
                new Email(email),
                "Concurrent",
                "Test",
                $"keycloak-{Guid.NewGuid():N}"
            );

            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();
            userId = user.Id.Value;
        }

        try
        {

            // Act - Simulate two concurrent contexts modifying the same user
            using var scope1 = Services.CreateScope();
            using var scope2 = Services.CreateScope();

            var context1 = scope1.ServiceProvider.GetRequiredService<UsersDbContext>();
            var context2 = scope2.ServiceProvider.GetRequiredService<UsersDbContext>();

            // EF Core translates .Where on converted property - use DbContext directly  
            var userFromContext1 = (await context1.Users.ToListAsync())
                .First(u => u.Username.Value == username);
            var userFromContext2 = (await context2.Users.ToListAsync())
                .First(u => u.Username.Value == username);

            // Modify in both contexts
            userFromContext1.UpdateProfile("Modified1", "User1");
            userFromContext2.UpdateProfile("Modified2", "User2");

            // First save succeeds
            await context1.SaveChangesAsync();

            // Assert - Second save should detect concurrency conflict
            var act = async () => await context2.SaveChangesAsync();

            await act.Should().ThrowAsync<DbUpdateConcurrencyException>(
                "PostgreSQL xmin concurrency token should detect conflicting updates");
        }
        finally
        {
            // Cleanup
            using var cleanupScope = Services.CreateScope();
            var cleanupContext = cleanupScope.ServiceProvider.GetRequiredService<UsersDbContext>();
            var userToDelete = await cleanupContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userToDelete != null)
            {
                cleanupContext.Users.Remove(userToDelete);
                await cleanupContext.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoConflict_ShouldSucceed()
    {
        // Arrange - Create user
        var username = $"noconflict_{Guid.NewGuid():N}"[..20];
        var email = $"{username}@test.com";
        Guid userId;

        using (var scope1 = Services.CreateScope())
        {
            var context1 = scope1.ServiceProvider.GetRequiredService<UsersDbContext>();

            var user = new User(
                new Username(username),
                new Email(email),
                "NoConflict",
                "Test",
                $"keycloak-{Guid.NewGuid():N}"
            );

            context1.Users.Add(user);
            await context1.SaveChangesAsync();
            userId = user.Id.Value;
        }

        try
        {
            // Act - Modify in fresh context (no conflict)
            using var scope2 = Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<UsersDbContext>();

            var userFromContext2 = (await context2.Users.ToListAsync())
                .First(u => u.Username.Value == username);
            userFromContext2.UpdateProfile("Updated", "User");

            // Assert - Should succeed (no concurrent modification)
            var act = async () => await context2.SaveChangesAsync();

            await act.Should().NotThrowAsync<DbUpdateConcurrencyException>(
                "sequential updates with no conflict should succeed");
        }
        finally
        {
            // Cleanup
            using var cleanupScope = Services.CreateScope();
            var cleanupContext = cleanupScope.ServiceProvider.GetRequiredService<UsersDbContext>();
            var userToDelete = await cleanupContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userToDelete != null)
            {
                cleanupContext.Users.Remove(userToDelete);
                await cleanupContext.SaveChangesAsync();
            }
        }
    }
}
