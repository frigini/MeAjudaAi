using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class ChangeUserEmailCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<User, UserId>> _userRepositoryMock;
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<ILogger<ChangeUserEmailCommandHandler>> _loggerMock;
    private readonly ChangeUserEmailCommandHandler _handler;

    public ChangeUserEmailCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IRepository<User, UserId>>();
        _userQueriesMock = new Mock<IUserQueries>();
        _loggerMock = new Mock<ILogger<ChangeUserEmailCommandHandler>>();

        _unitOfWorkMock
            .Setup(x => x.GetRepository<User, UserId>())
            .Returns(_userRepositoryMock.Object);

        _handler = new ChangeUserEmailCommandHandler(
            _unitOfWorkMock.Object,
            _userQueriesMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserEmailCommand(
            UserId: userId,
            NewEmail: "newemail@example.com",
            UpdatedBy: null);

        var existingUser = new UserBuilder()
            .WithId(new UserId(userId))
            .WithEmail("old@example.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userQueriesMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("newemail@example.com");

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);

        _userQueriesMock.Verify(
            x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
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
        var command = new ChangeUserEmailCommand(
            UserId: userId,
            NewEmail: "newemail@example.com",
            UpdatedBy: null);

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
            x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserEmailCommand(
            UserId: userId,
            NewEmail: "taken@example.com",
            UpdatedBy: null);

        var existingUser = new UserBuilder()
            .WithId(new UserId(userId))
            .WithEmail("old@example.com")
            .Build();

        var otherUser = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("taken@example.com")
            .Build();

        _userRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userQueriesMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Email address is already in use");

        _userRepositoryMock.Verify(
            x => x.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()),
            Times.Once);

        _userQueriesMock.Verify(
            x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangeUserEmailCommand(
            UserId: userId,
            NewEmail: "newemail@example.com",
            UpdatedBy: null);

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