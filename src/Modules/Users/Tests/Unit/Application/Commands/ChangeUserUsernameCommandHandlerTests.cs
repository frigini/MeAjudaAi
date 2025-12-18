using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Tests.Mocks;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class ChangeUserUsernameCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly MockDateTimeProvider _dateTimeProvider;
    private readonly Mock<ILogger<ChangeUserUsernameCommandHandler>> _loggerMock;
    private readonly ChangeUserUsernameCommandHandler _handler;

    public ChangeUserUsernameCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _dateTimeProvider = new MockDateTimeProvider();
        _loggerMock = new Mock<ILogger<ChangeUserUsernameCommandHandler>>();
        _handler = new ChangeUserUsernameCommandHandler(_userRepositoryMock.Object, _dateTimeProvider, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ShouldChangeUsernameSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new ChangeUserUsernameCommand(userId, newUsername, "admin");

        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@test.com")
            .WithFirstName("Test")
            .WithLastName("User")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be(newUsername);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserUsernameCommand(userId, "newusername");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("User not found");

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.IsAny<Username>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UsernameAlreadyTaken_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var newUsername = "existingusername";
        var command = new ChangeUserUsernameCommand(userId, newUsername);

        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@test.com")
            .Build();

        var existingUser = new UserBuilder()
            .WithUsername(newUsername)
            .WithEmail("existing@test.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("Username is already taken by another user");

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SameUserWithSameUsername_ShouldChangeUsernameSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "sameusername";
        var command = new ChangeUserUsernameCommand(userId, newUsername);

        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@test.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user); // Mesmo usuário

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be(newUsername);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RateLimitExceeded_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new ChangeUserUsernameCommand(userId, newUsername, BypassRateLimit: false);

        // Para simular rate limit, vamos criar um user que teve mudança recente
        var recentUser = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@test.com")
            .Build();

        // Simular que o usuário mudou o username recentemente através do método ChangeUsername
        // Isso irá definir LastUsernameChangeAt para o momento atual
        var mockDateTimeProvider = new MockDateTimeProvider();
        recentUser.ChangeUsername("tempusername", mockDateTimeProvider); // Simula mudança recente

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentUser);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("Username can only be changed once per month");

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BypassRateLimit_ShouldChangeUsernameSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new ChangeUserUsernameCommand(userId, newUsername, "admin", BypassRateLimit: true);

        // Simular usuário que mudou username recentemente, mas com bypass
        var recentUser = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@test.com")
            .Build();

        // Simular mudança recente
        var mockDateTimeProvider2 = new MockDateTimeProvider();
        recentUser.ChangeUsername("tempusername", mockDateTimeProvider2);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentUser);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be(newUsername);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserUsernameCommand(userId, "newusername");
        var exceptionMessage = "Database connection failed";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().StartWith("Failed to change username:");

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UpdateRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new ChangeUserUsernameCommand(userId, newUsername);

        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@test.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == newUsername), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().StartWith("Failed to change username:");
    }

    [Fact]
    public async Task HandleAsync_CancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserUsernameCommand(userId, "newusername");
        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _handler.HandleAsync(command, cancellationTokenSource.Token);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().StartWith("Failed to change username:");
    }
}
