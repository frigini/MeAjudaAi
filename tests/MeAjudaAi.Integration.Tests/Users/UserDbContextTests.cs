using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;

namespace MeAjudaAi.Integration.Tests.Users;

/// <summary>
/// Testes para verificar se o DbContext está funcionando corretamente
/// </summary>
public class UserDbContextTests : ApiTestBase
{
    [Fact]
    public async Task CanConnectToDatabase_ShouldWork()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Act & Assert
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task CanSaveAndRetrieveUser_ShouldWork()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var user = new User(
            new Username("testuser"),
            new Email("test@example.com"),
            "Test",
            "User",
            "keycloak-id-123"
        );

        // Act
        context.Users.Add(user);
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);

        var savedUser = await context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Username.Value.Should().Be("testuser");
        savedUser.Email.Value.Should().Be("test@example.com");
    }
}
