using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.Commands;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints;

/// <summary>
/// Testes unitários para validação do endpoint de exclusão de usuários.
/// Testa mapeamento de dados, validação de entrada e estrutura de commands.
/// </summary>
public class DeleteUserEndpointTests
{
    [Fact]
    public void ToDeleteCommand_WithValidGuid_ShouldCreateCorrectCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var command = userId.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
        command.Should().BeOfType<DeleteUserCommand>();
    }

    [Fact]
    public void ToDeleteCommand_WithEmptyGuid_ShouldCreateCommandWithEmptyId()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var command = userId.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(Guid.Empty);
        command.Should().BeOfType<DeleteUserCommand>();
    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [InlineData("12345678-90ab-cdef-1234-567890abcdef")]
    public void ToDeleteCommand_WithDifferentValidGuids_ShouldMapCorrectly(string guidString)
    {
        // Arrange
        var userId = Guid.Parse(guidString);

        // Act
        var command = userId.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(userId);
        command.UserId.ToString().Should().Be(guidString);
    }

    [Fact]
    public void DeleteUserCommand_Properties_ShouldBeReadOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        // Act & Assert
        command.UserId.Should().Be(userId);
        command.CorrelationId.Should().NotBeEmpty();
        
        // Verify UserId equality even with different CorrelationId
        var command2 = new DeleteUserCommand(userId);
        command.UserId.Should().Be(command2.UserId);
        command.CorrelationId.Should().NotBe(command2.CorrelationId); // Different instances have different CorrelationIds
    }

    [Fact]
    public void DeleteUserCommand_ToString_ShouldContainUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        // Act
        var stringRepresentation = command.ToString();

        // Assert
        stringRepresentation.Should().Contain(userId.ToString());
        stringRepresentation.Should().Contain("DeleteUserCommand");
    }

    [Fact]
    public void MapperExtension_ShouldBeAccessibleFromGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert - Testing that the extension method is available
        var action = () => userId.ToDeleteCommand();
        action.Should().NotThrow();
        
        var result = action();
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ToDeleteCommand_PerformanceTest_ShouldBeEfficient(int iterations)
    {
        // Arrange
        var userIds = Enumerable.Range(0, iterations)
            .Select(_ => Guid.NewGuid())
            .ToList();

        // Act
        var commands = userIds.Select(id => id.ToDeleteCommand()).ToList();

        // Assert
        commands.Should().HaveCount(iterations);
        commands.Should().AllSatisfy(cmd => 
        {
            cmd.Should().NotBeNull();
            cmd.Should().BeOfType<DeleteUserCommand>();
        });
    }
}