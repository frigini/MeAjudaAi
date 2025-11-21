using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class ChangeUserEmailCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<ChangeUserEmailCommandHandler>> _loggerMock;
    private readonly ChangeUserEmailCommandHandler _handler;

    public ChangeUserEmailCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<ChangeUserEmailCommandHandler>>();
        _handler = new ChangeUserEmailCommandHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ShouldChangeEmailSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newEmail = "newemail@test.com";
        var command = new ChangeUserEmailCommand(userId, newEmail, "admin");

        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("oldemail@test.com")
            .WithFirstName("Test")
            .WithLastName("User")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.Is<Email>(e => e.Value == newEmail), It.IsAny<CancellationToken>()))
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
        result.Value!.Email.Should().Be(newEmail);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.Is<Email>(e => e.Value == newEmail), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserEmailCommand(userId, "newemail@test.com");

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
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmailAlreadyInUse_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var newEmail = "existing@test.com";
        var command = new ChangeUserEmailCommand(userId, newEmail);

        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("oldemail@test.com")
            .Build();

        var existingUser = new UserBuilder()
            .WithUsername("existinguser")
            .WithEmail(newEmail)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.Is<Email>(e => e.Value == newEmail), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("Email address is already in use by another user");

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.Is<Email>(e => e.Value == newEmail), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SameUserWithSameEmail_ShouldChangeEmailSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newEmail = "sameemail@test.com";
        var command = new ChangeUserEmailCommand(userId, newEmail);

        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("oldemail@test.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.Is<Email>(e => e.Value == newEmail), It.IsAny<CancellationToken>()))
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
        result.Value!.Email.Should().Be(newEmail);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserEmailCommand(userId, "newemail@test.com");
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
        result.Error.Message.Should().StartWith("Failed to change user email:");

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserEmailCommand(userId, "newemail@test.com");
#pragma warning disable CA2000 // CancellationTokenSource em teste é descartado ao fim do método
        var cancellationTokenSource = new CancellationTokenSource();
#pragma warning restore CA2000
        await cancellationTokenSource.CancelAsync();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        var result = await _handler.HandleAsync(command, cancellationTokenSource.Token);

        // O handler captura a exceção e retorna failure
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().StartWith("Failed to change user email:");
    }

    [Fact]
    public async Task HandleAsync_UpdateRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newEmail = "newemail@test.com";
        var command = new ChangeUserEmailCommand(userId, newEmail);

        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("oldemail@test.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.Is<Email>(e => e.Value == newEmail), It.IsAny<CancellationToken>()))
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
        result.Error.Message.Should().StartWith("Failed to change user email:");
    }
}
