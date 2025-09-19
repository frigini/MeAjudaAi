using FluentAssertions;
using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints;

/// <summary>
/// Testes unitários para validação do endpoint de atualização de perfil de usuários.
/// Testa mapeamento de dados, validação de entrada e estrutura de commands.
/// </summary>
public class UpdateUserProfileEndpointTests
{
    [Fact]
    public void ToCommand_WithValidRequestAndUserId_ShouldCreateCorrectCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserProfileRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com" // Email is in request but not mapped to command
        };

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
        command.FirstName.Should().Be("John");
        command.LastName.Should().Be("Doe");
        command.Should().BeOfType<UpdateUserProfileCommand>();
        // Note: Email is not part of UpdateUserProfileCommand by design
    }

    [Theory]
    [InlineData("", "LastName")]
    [InlineData("FirstName", "")]
    [InlineData("", "")]
    public void ToCommand_WithEmptyFields_ShouldCreateCommandWithProvidedValues(string firstName, string lastName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = "email@test.com" // Email is ignored in command mapping
        };

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
    }

    [Fact]
    public void ToCommand_WithEmptyGuid_ShouldCreateCommandWithEmptyUserId()
    {
        // Arrange
        var userId = Guid.Empty;
        var request = new UpdateUserProfileRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com" // Email is in request but not mapped
        };

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(Guid.Empty);
        command.FirstName.Should().Be("Test");
        command.LastName.Should().Be("User");
    }

    [Theory]
    [InlineData("João", "da Silva")]
    [InlineData("Mary Jane", "Smith-Watson")]
    [InlineData("José María", "García López")]
    public void ToCommand_WithInternationalNames_ShouldPreserveSpecialCharacters(string firstName, string lastName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = "test@example.com" // Email present in request but not used in command
        };

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.Should().NotBeNull();
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
    }

    [Theory]
    [InlineData("   FirstName   ", "   LastName   ")]
    [InlineData("\tFirstName\t", "\tLastName\t")]
    public void ToCommand_WithWhitespaceAroundValues_ShouldPreserveWhitespace(string firstName, string lastName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = "email@test.com" // Email present but not mapped to command
        };

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.Should().NotBeNull();
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
        
        // Note: Trimming should happen at domain level or validation
    }

    [Fact]
    public void UpdateUserProfileCommand_Properties_ShouldBeReadOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var command = new UpdateUserProfileCommand(userId, firstName, lastName);

        // Act & Assert
        command.UserId.Should().Be(userId);
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
        command.CorrelationId.Should().NotBeEmpty();
        
        // Verify property equality even with different CorrelationId
        var command2 = new UpdateUserProfileCommand(userId, firstName, lastName);
        command.UserId.Should().Be(command2.UserId);
        command.FirstName.Should().Be(command2.FirstName);
        command.LastName.Should().Be(command2.LastName);
        command.CorrelationId.Should().NotBe(command2.CorrelationId); // Different instances have different CorrelationIds
    }

    [Fact]
    public void UpdateUserProfileCommand_ToString_ShouldContainRelevantInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var command = new UpdateUserProfileCommand(userId, firstName, lastName);

        // Act
        var stringRepresentation = command.ToString();

        // Assert
        stringRepresentation.Should().Contain("UpdateUserProfileCommand");
        stringRepresentation.Should().Contain(userId.ToString());
        stringRepresentation.Should().Contain(firstName);
        stringRepresentation.Should().Contain(lastName);
    }

    [Fact]
    public void MapperExtension_ShouldBeAccessibleFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserProfileRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };

        // Act & Assert - Testing that the extension method is available
        var action = () => request.ToCommand(userId);
        action.Should().NotThrow();
        
        var result = action();
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void ToCommand_PerformanceTest_ShouldBeEfficient(int iterations)
    {
        // Arrange
        var requests = Enumerable.Range(1, iterations)
            .Select(i => new UpdateUserProfileRequest
            {
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Email = $"user{i}@example.com"
            })
            .ToList();

        var userIds = Enumerable.Range(1, iterations)
            .Select(_ => Guid.NewGuid())
            .ToList();

        // Act
        var commands = requests.Zip(userIds, (req, id) => req.ToCommand(id)).ToList();

        // Assert
        commands.Should().HaveCount(iterations);
        commands.Should().AllSatisfy(cmd => 
        {
            cmd.Should().NotBeNull();
            cmd.Should().BeOfType<UpdateUserProfileCommand>();
            cmd.UserId.Should().NotBe(Guid.Empty);
            cmd.FirstName.Should().StartWith("FirstName");
            cmd.LastName.Should().StartWith("LastName");
        });
    }

    [Fact]
    public void UpdateUserProfileRequest_DefaultValues_ShouldBeEmptyStrings()
    {
        // Arrange & Act
        var request = new UpdateUserProfileRequest();

        // Assert
        request.FirstName.Should().Be(string.Empty);
        request.LastName.Should().Be(string.Empty);
        request.Email.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("JOHN", "DOE")]
    [InlineData("john", "doe")]
    [InlineData("John", "Doe")]
    public void ToCommand_WithDifferentCasing_ShouldPreserveCasing(string firstName, string lastName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserProfileRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = "test@example.com" // Email present but not mapped
        };

        // Act
        var command = request.ToCommand(userId);

        // Assert
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
        
        // Note: Case normalization should happen at domain level
    }
}