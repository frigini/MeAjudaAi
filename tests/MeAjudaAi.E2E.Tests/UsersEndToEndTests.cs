using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace MeAjudaAi.E2E.Tests.Users;

/// <summary>
/// Testes E2E para o módulo de usuários usando TestContainers
/// Demonstra como usar a TestContainerTestBase
/// </summary>
public class UsersEndToEndTests : TestContainerTestBase
{
    [Fact]
    public async Task CreateUser_Should_Return_Success()
    {
        // Arrange
        var createUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            KeycloakId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await PostJsonAsync("/api/v1/users", createUserRequest);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 201 Created but got {response.StatusCode}. Response: {content}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/v1/users");
    }

    [Fact]
    public async Task GetUsers_Should_Return_Paginated_Results()
    {
        // Arrange - Criar alguns usuários primeiro
        await CreateTestUsersAsync(3);

        // Act
        var response = await ApiClient.GetAsync("/api/v1/users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<object>(content, JsonOptions);
        result.Should().NotBeNull();
    }

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

    /// <summary>
    /// Helper para criar usuários de teste
    /// </summary>
    private async Task CreateTestUsersAsync(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var createUserRequest = new
            {
                Username = Faker.Internet.UserName(),
                Email = Faker.Internet.Email(),
                FirstName = Faker.Name.FirstName(),
                LastName = Faker.Name.LastName(),
                KeycloakId = Guid.NewGuid().ToString()
            };

            await PostJsonAsync("/api/v1/users", createUserRequest);
        }
    }
}