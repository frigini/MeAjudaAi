using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Layer", "API")]
[Trait("Component", "Mappers")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_WithValidCreateUserRequest_ShouldMapToCreateUserCommand()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123",
            Roles = ["Customer", "User"]
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.Username.Should().Be("testuser");
        command.Email.Should().Be("test@example.com");
        command.FirstName.Should().Be("John");
        command.LastName.Should().Be("Doe");
        command.Password.Should().Be("password123");
        command.Roles.Should().BeEquivalentTo(["Customer", "User"]);
    }

    [Fact]
    public void ToCommand_WithNullRoles_ShouldMapToEmptyArray()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123",
            Roles = null
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.Roles.Should().NotBeNull();
        command.Roles.Should().BeEmpty();
    }

    [Fact]
    public void ToCommand_WithUpdateUserProfileRequest_ShouldMapToUpdateCommand()
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "Jane",
            LastName = "Smith"
        };
        var userId = Guid.NewGuid();

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
        command.FirstName.Should().Be("Jane");
        command.LastName.Should().Be("Smith");
    }

    [Fact]
    public void ToDeleteCommand_WithUserId_ShouldMapToDeleteUserCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var command = userId.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
    }

    [Fact]
    public void ToQuery_WithUserId_ShouldMapToGetUserByIdQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var query = userId.ToQuery();

        // Assert
        query.Should().NotBeNull();
        query.UserId.Should().Be(userId);
    }

    [Fact]
    public void ToEmailQuery_WithValidEmail_ShouldMapToGetUserByEmailQuery()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var query = email.ToEmailQuery();

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(email);
    }

    [Fact]
    public void ToEmailQuery_WithNullEmail_ShouldMapToEmptyStringQuery()
    {
        // Arrange
        string? email = null;

        // Act
        var query = email.ToEmailQuery();

        // Assert
        query.Should().NotBeNull();
        query.Email.Should().Be(string.Empty);
    }

    [Fact]
    public void ToUsersQuery_WithValidGetUsersRequest_ShouldMapCorrectly()
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 2,
            PageSize = 25,
            SearchTerm = "john"
        };

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(2);
        query.PageSize.Should().Be(25);
        query.SearchTerm.Should().Be("john");
    }

    [Fact]
    public void ToUsersQuery_WithNullSearchTerm_ShouldMapCorrectly()
    {
        // Arrange
        var request = new GetUsersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = null
        };

        // Act
        var query = request.ToUsersQuery();

        // Assert
        query.Should().NotBeNull();
        query.Page.Should().Be(1);
        query.PageSize.Should().Be(10);
        query.SearchTerm.Should().BeNull();
    }

    [Fact]
    public void ToCommand_WithEmptyStrings_ShouldMapCorrectly()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "",
            Email = "",
            FirstName = "",
            LastName = "",
            Password = "",
            Roles = Array.Empty<string>()
        };

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.Username.Should().Be("");
        command.Email.Should().Be("");
        command.FirstName.Should().Be("");
        command.LastName.Should().Be("");
        command.Password.Should().Be("");
        command.Roles.Should().BeEmpty();
    }

    [Fact]
    public void ToCommand_WithWhitespaceStrings_ShouldMapCorrectly()
    {
        // Arrange
        var request = new UpdateUserProfileRequest
        {
            FirstName = "   ",
            LastName = "   "
        };
        var userId = Guid.NewGuid();

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
        command.FirstName.Should().Be("   ");
        command.LastName.Should().Be("   ");
    }
}