using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class ChangeUserUsernameCommandHandlerTests
{
    private readonly Mock<IUserUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<User, UserId>> _userRepositoryMock;
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly FakeTimeProvider _dateTimeProvider;
    private readonly Mock<ILogger<ChangeUserUsernameCommandHandler>> _loggerMock;
    private readonly ChangeUserUsernameCommandHandler _handler;

    public ChangeUserUsernameCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUserUnitOfWork>();
        _userRepositoryMock = new Mock<IRepository<User, UserId>>();
        _userQueriesMock = new Mock<IUserQueries>();
        _dateTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _loggerMock = new Mock<ILogger<ChangeUserUsernameCommandHandler>>();

        _unitOfWorkMock
            .Setup(x => x.GetRepository<User, UserId>())
            .Returns(_userRepositoryMock.Object);

        _handler = new ChangeUserUsernameCommandHandler(
            _unitOfWorkMock.Object,
            _userQueriesMock.Object,
            _dateTimeProvider,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserUsernameCommand(
            UserId: userId,
            NewUsername: "newusername",
            UpdatedBy: null,
            BypassRateLimit: true);

        var existingUser = new UserBuilder()
            .WithId(new UserId(userId))
            .WithUsername("oldusername")
            .Build();

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userQueriesMock
            .Setup(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be("newusername");

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);

        _userQueriesMock.Verify(
            x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserUsernameCommand(
            UserId: userId,
            NewUsername: "newusername",
            UpdatedBy: null,
            BypassRateLimit: true);

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

        _userQueriesMock.Verify(
            x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUsername_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserUsernameCommand(
            UserId: userId,
            NewUsername: "takenusername",
            UpdatedBy: null,
            BypassRateLimit: true);

        var existingUser = new UserBuilder()
            .WithId(new UserId(userId))
            .WithUsername("oldusername")
            .Build();

        var otherUser = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithUsername("takenusername")
            .Build();

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userQueriesMock
            .Setup(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Username is already taken");

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);

        _userQueriesMock.Verify(
            x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserUsernameCommand(
            UserId: userId,
            NewUsername: "newusername",
            UpdatedBy: null,
            BypassRateLimit: true);

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}


