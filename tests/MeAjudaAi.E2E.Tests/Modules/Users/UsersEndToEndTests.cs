using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules.Users;

/// <summary>
/// Testes E2E para o módulo de usuários usando TestContainers
/// Demonstra como usar a TestContainerTestBase
/// NOTE: Basic CRUD tests removed - duplicates UsersIntegrationTests
/// Focuses on database persistence and complex scenarios
/// </summary>
public class UsersEndToEndTests : TestContainerTestBase
{
    // NOTE: CreateUser_Should_Return_Success removed - duplicates UsersIntegrationTests.CreateUser_WithValidData_ShouldReturnCreated
    // NOTE: GetUsers_Should_Return_Paginated_Results removed - duplicates UsersIntegrationTests.GetUsers_ShouldReturnOkWithPaginatedResult

    [Fact]
    public async Task Database_Should_Persist_Users_Correctly()
    {
        // Arrange
        var username = new Username(Faker.Internet.UserName());
        var email = new Email(Faker.Internet.Email());

        // Act - Criar usuário diretamente no banco
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<UsersDbContext>();

            var user = new User(
                username: username,
                email: email,
                firstName: Faker.Name.FirstName(),
                lastName: Faker.Name.LastName(),
                keycloakId: Guid.NewGuid().ToString()
            );

            context.Users.Add(user);
            await context.SaveChangesAsync();
        });

        // Assert - Verificar se o usuário foi persistido
        await WithServiceScopeAsync(async services =>
        {
            var context = services.GetRequiredService<UsersDbContext>();

            var foundUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            foundUser.Should().NotBeNull();
            foundUser!.Email.Should().Be(email);
        });
    }
}
