using MeAjudaAi.Modules.Users.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints;

/// <summary>
/// Testes unitários para validação de entrada do endpoint de criação de usuários.
/// Foca na validação de dados de entrada e estrutura de requests.
/// </summary>
public class CreateUserEndpointTests
{
    [Fact]
    public void CreateUserRequest_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Test123!@#",
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Username.Should().Be("testuser");
        request.Password.Should().Be("Test123!@#");
        request.FirstName.Should().Be("Test");
        request.LastName.Should().Be("User");
    }

    [Theory]
    [InlineData("", "username", "password", "FirstName", "LastName")] // Email vazio
    [InlineData("test@example.com", "", "password", "FirstName", "LastName")] // Username vazio
    [InlineData("test@example.com", "username", "", "FirstName", "LastName")] // Password vazio
    [InlineData("test@example.com", "username", "password", "", "LastName")] // FirstName vazio
    [InlineData("test@example.com", "username", "password", "FirstName", "")] // LastName vazio
    public void CreateUserRequest_WithMissingRequiredFields_ShouldAllowCreation(
        string email, string username, string password, string firstName, string lastName)
    {
        // Arrange & Act
        var request = new CreateUserRequest
        {
            Email = email,
            Username = username,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        // Assert - Validação será feita na camada de aplicação
        request.Should().NotBeNull();
        request.Email.Should().Be(email);
        request.Username.Should().Be(username);
        request.Password.Should().Be(password);
        request.FirstName.Should().Be(firstName);
        request.LastName.Should().Be(lastName);
    }

    [Fact]
    public void CreateUserRequest_DefaultValues_ShouldBeEmpty()
    {
        // Arrange & Act
        var request = new CreateUserRequest();

        // Assert
        request.Email.Should().Be(string.Empty);
        request.Username.Should().Be(string.Empty);
        request.Password.Should().Be(string.Empty);
        request.FirstName.Should().Be(string.Empty);
        request.LastName.Should().Be(string.Empty);
        request.Roles.Should().BeNull();
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("123@numbers.com")]
    public void CreateUserRequest_WithDifferentEmailFormats_ShouldAcceptValue(string email)
    {
        // Arrange & Act
        var request = new CreateUserRequest
        {
            Email = email,
            Username = "testuser",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        request.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("test_user")]
    [InlineData("user-name")]
    public void CreateUserRequest_WithDifferentUsernameFormats_ShouldAcceptValue(string username)
    {
        // Arrange & Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Username = username,
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        request.Username.Should().Be(username);
    }

    [Fact]
    public void CreateUserRequest_WithRoles_ShouldAcceptValue()
    {
        // Arrange
        var roles = new[] { "Admin", "User", "Moderator" };

        // Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            Roles = roles
        };

        // Assert
        request.Roles.Should().NotBeNull();
        request.Roles.Should().BeEquivalentTo(roles);
        request.Roles.Should().HaveCount(3);
    }
}