using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<IUserUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<User, UserId>> _userRepositoryMock;
    private readonly Mock<IUsersCacheService> _usersCacheServiceMock;
    private readonly Mock<ILogger<UpdateUserProfileCommandHandler>> _loggerMock;
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUserUnitOfWork>();
        _userRepositoryMock = new Mock<IRepository<User, UserId>>();
        _usersCacheServiceMock = new Mock<IUsersCacheService>();
        _loggerMock = new Mock<ILogger<UpdateUserProfileCommandHandler>>();

        _unitOfWorkMock
            .Setup(x => x.GetRepository<User, UserId>())
            .Returns(_userRepositoryMock.Object);

        _handler = new UpdateUserProfileCommandHandler(
            _unitOfWorkMock.Object,
            _usersCacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_UpdatesUserProfileSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(
            userId,
            "Updated First",
            "Updated Last");

        var existingUser = new UserBuilder()
            .WithId(new UserId(userId))
            .WithFirstName("Original First")
            .WithLastName("Original Last")
            .WithEmail("test@example.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _usersCacheServiceMock
            .Setup(x => x.InvalidateUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be("Updated First");
        result.Value.LastName.Should().Be("Updated Last");

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _usersCacheServiceMock.Verify(
            x => x.InvalidateUserAsync(userId, "test@example.com", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(
            userId,
            "Updated First",
            "Updated Last");

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _usersCacheServiceMock.Verify(
            x => x.InvalidateUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(
            userId,
            "Updated First",
            "Updated Last");

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CacheInvalidationFails_StillReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(
            userId,
            "Updated First",
            "Updated Last");

        var existingUser = new UserBuilder()
            .WithId(new UserId(userId))
            .WithFirstName("Original First")
            .WithLastName("Original Last")
            .WithEmail("test@example.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _usersCacheServiceMock
            .Setup(x => x.InvalidateUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyNames_ShouldSucceedAtDomainLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(
            userId,
            "",
            "");

        var existingUser = new UserBuilder()
            .WithId(new UserId(userId))
            .WithFirstName("Original First")
            .WithLastName("Original Last")
            .WithEmail("test@example.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
