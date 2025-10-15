using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints.UserAdmin;

[Trait("Category", "Unit")]
public class DeleteUserEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock;

    public DeleteUserEndpointTests()
    {
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
    }

    [Fact]
    public void DeleteUserEndpoint_ShouldInheritFromBaseEndpoint()
    {
        // Arrange & Act
        var endpointType = typeof(DeleteUserEndpoint);

        // Assert
        endpointType.BaseType?.Name.Should().Be("BaseEndpoint");
    }

    [Fact]
    public void DeleteUserEndpoint_ShouldImplementIEndpoint()
    {
        // Arrange & Act
        var endpointType = typeof(DeleteUserEndpoint);

        // Assert
        endpointType.GetInterface("IEndpoint").Should().NotBeNull();
    }

    [Fact]
    public void Map_ShouldBeStaticMethod()
    {
        // Arrange
        var mapMethod = typeof(DeleteUserEndpoint).GetMethod("Map", BindingFlags.Public | BindingFlags.Static);

        // Assert
        mapMethod.Should().NotBeNull();
        mapMethod!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var expectedCommand = new DeleteUserCommand(userId);
        var successResult = Result.Success();

        _commandDispatcherMock
            .Setup(x => x.SendAsync<DeleteUserCommand, Result>(
                It.Is<DeleteUserCommand>(cmd => cmd.UserId == userId),
                cancellationToken))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeDeleteUserAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var httpResult = result as IStatusCodeHttpResult;
        httpResult?.StatusCode.Should().Be(StatusCodes.Status204NoContent);

        _commandDispatcherMock.Verify(
            x => x.SendAsync<DeleteUserCommand, Result>(
                It.Is<DeleteUserCommand>(cmd => cmd.UserId == userId),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var notFoundResult = Error.NotFound("User not found");

        _commandDispatcherMock
            .Setup(x => x.SendAsync<DeleteUserCommand, Result>(
                It.Is<DeleteUserCommand>(cmd => cmd.UserId == userId),
                cancellationToken))
            .ReturnsAsync(notFoundResult);

        // Act
        var result = await InvokeDeleteUserAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var httpResult = result as IStatusCodeHttpResult;
        httpResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteUserAsync_WithInternalError_ShouldReturnInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var internalError = Error.Internal("Internal server error");

        _commandDispatcherMock
            .Setup(x => x.SendAsync<DeleteUserCommand, Result>(
                It.Is<DeleteUserCommand>(cmd => cmd.UserId == userId),
                cancellationToken))
            .ReturnsAsync(internalError);

        // Act
        var result = await InvokeDeleteUserAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var httpResult = result as IStatusCodeHttpResult;
        httpResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task DeleteUserAsync_WithCancellationToken_ShouldPassTokenToDispatcher()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var successResult = Result.Success();

        _commandDispatcherMock
            .Setup(x => x.SendAsync<DeleteUserCommand, Result>(
                It.IsAny<DeleteUserCommand>(),
                cancellationToken))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeDeleteUserAsync(userId, cancellationToken);

        // Assert
        _commandDispatcherMock.Verify(
            x => x.SendAsync<DeleteUserCommand, Result>(
                It.IsAny<DeleteUserCommand>(),
                cancellationToken),
            Times.Once);
    }

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
    public void ToDeleteCommand_WithEmptyGuid_ShouldCreateCommandWithEmptyGuid()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var command = userId.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToDeleteCommand_ShouldAlwaysCreateNewInstance()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var command1 = userId.ToDeleteCommand();
        var command2 = userId.ToDeleteCommand();

        // Assert
        command1.Should().NotBeSameAs(command2);
        command1.Should().BeEquivalentTo(command2, options => options.Excluding(x => x.CorrelationId));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-5678-9012-123456789012")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public void ToDeleteCommand_WithDifferentGuids_ShouldCreateCorrectCommands(string guidString)
    {
        // Arrange
        var userId = Guid.Parse(guidString);

        // Act
        var command = userId.ToDeleteCommand();

        // Assert
        command.UserId.Should().Be(userId);
    }

    private async Task<IResult> InvokeDeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleteUserAsyncMethod = typeof(DeleteUserEndpoint)
            .GetMethod("DeleteUserAsync", BindingFlags.NonPublic | BindingFlags.Static);

        deleteUserAsyncMethod.Should().NotBeNull("DeleteUserAsync method should exist");

        var task = (Task<IResult>)deleteUserAsyncMethod!.Invoke(null, [id, _commandDispatcherMock.Object, cancellationToken])!;
        return await task;
    }
}