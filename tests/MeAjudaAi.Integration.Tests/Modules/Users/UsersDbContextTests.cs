using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

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

    // NOTE: CanSaveAndRetrieveUser removed - duplicates UserRepositoryIntegrationTests.AddAsync_ShouldPersistUser
    // DbContext tests should focus on schema/configuration, not CRUD operations
}
