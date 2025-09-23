using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserDomainService> _userDomainServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<DeleteUserCommandHandler>> _loggerMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userDomainServiceMock = new Mock<IUserDomainService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.CurrentDate()).Returns(DateTime.UtcNow);
        _loggerMock = new Mock<ILogger<DeleteUserCommandHandler>>();
        _handler = new DeleteUserCommandHandler(_userRepositoryMock.Object, _userDomainServiceMock.Object, _dateTimeProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(UserId: userId);

        var existingUser = new UserBuilder()
            .WithId(userId)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userDomainServiceMock
            .Setup(x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _userDomainServiceMock.Verify(
            x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(UserId: userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("User not found");
        
        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _userDomainServiceMock.Verify(
            x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _userRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithKeycloakSyncFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(UserId: userId);

        var existingUser = new UserBuilder()
            .WithId(userId)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userDomainServiceMock
            .Setup(x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Keycloak sync failed"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Keycloak sync failed");
        
        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _userDomainServiceMock.Verify(
            x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _userRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithRepositoryException_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(UserId: userId);

        var existingUser = new UserBuilder()
            .WithId(userId)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userDomainServiceMock
            .Setup(x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be($"Failed to delete user: Database error");
        
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}