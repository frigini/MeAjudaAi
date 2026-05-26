using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<User, UserId>> _userRepositoryMock;
    private readonly Mock<IUserDomainService> _userDomainServiceMock;
    private readonly Mock<IUsersCacheService> _usersCacheServiceMock;
    private readonly FakeTimeProvider _dateTimeProvider;
    private readonly Mock<ILogger<DeleteUserCommandHandler>> _loggerMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IRepository<User, UserId>>();
        _userDomainServiceMock = new Mock<IUserDomainService>();
        _usersCacheServiceMock = new Mock<IUsersCacheService>();
        _dateTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _loggerMock = new Mock<ILogger<DeleteUserCommandHandler>>();

        _unitOfWorkMock
            .Setup(x => x.GetRepository<User, UserId>())
            .Returns(_userRepositoryMock.Object);

        _handler = new DeleteUserCommandHandler(
            _unitOfWorkMock.Object,
            _userDomainServiceMock.Object,
            _usersCacheServiceMock.Object,
            _dateTimeProvider,
            _loggerMock.Object);
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
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userDomainServiceMock
            .Setup(x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _userDomainServiceMock.Verify(
            x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _usersCacheServiceMock.Verify(
            x => x.InvalidateUserAsync(userId, It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(UserId: userId);

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.NotFound.User);

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _userDomainServiceMock.Verify(
            x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _usersCacheServiceMock.Verify(
            x => x.InvalidateUserAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
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
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
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
            x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _userDomainServiceMock.Verify(
            x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _usersCacheServiceMock.Verify(
            x => x.InvalidateUserAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
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
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userDomainServiceMock
            .Setup(x => x.SyncUserWithKeycloakAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to delete user: Database error");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _usersCacheServiceMock.Verify(
            x => x.InvalidateUserAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
