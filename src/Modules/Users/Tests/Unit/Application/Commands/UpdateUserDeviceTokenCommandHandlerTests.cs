using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Commands;

public class UpdateUserDeviceTokenCommandHandlerTests
{
    private readonly Mock<IUserUnitOfWork> _uowMock;
    private readonly Mock<IRepository<User, UserId>> _repositoryMock;
    private readonly Mock<IUsersCacheService> _cacheServiceMock;
    private readonly UpdateUserDeviceTokenCommandHandler _handler;

    public UpdateUserDeviceTokenCommandHandlerTests()
    {
        _uowMock = new Mock<IUserUnitOfWork>();
        _repositoryMock = new Mock<IRepository<User, UserId>>();
        _cacheServiceMock = new Mock<IUsersCacheService>();

        _uowMock.Setup(u => u.GetRepository<User, UserId>()).Returns(_repositoryMock.Object);
        _handler = new UpdateUserDeviceTokenCommandHandler(_uowMock.Object, _cacheServiceMock.Object, Mock.Of<ILogger<UpdateUserDeviceTokenCommandHandler>>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ShouldSaveAndInvalidateCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create(new Username("user"), new Email("test@test.com"), "first", "last", Guid.NewGuid().ToString()).Value!;
        user.SetIdForTesting(new UserId(userId));
        
        _repositoryMock.Setup(r => r.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateUserDeviceTokenCommand(userId, "new-token", Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DeviceToken.Should().Be("new-token");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.InvalidateUserAsync(userId, user.Email.Value, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldReturn404()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.TryFindAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new UpdateUserDeviceTokenCommand(userId, "token", Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WhenClearingToken_ShouldPersistNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create(new Username("user"), new Email("test@test.com"), "first", "last", Guid.NewGuid().ToString()).Value!;
        user.SetIdForTesting(new UserId(userId));
        user.UpdateDeviceToken("existing-token");
        
        _repositoryMock.Setup(r => r.TryFindAsync(It.Is<UserId>(id => id.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateUserDeviceTokenCommand(userId, null, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DeviceToken.Should().BeNull();
    }
}
