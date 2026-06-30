using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<User, UserId>> _userRepositoryMock;
    private readonly Mock<IUserDomainService> _userDomainServiceMock;
    private readonly Mock<IUsersCacheService> _usersCacheServiceMock;
    private readonly FakeTimeProvider _dateTimeProvider;
    private readonly Mock<ILogger<DeleteUserCommandHandler>> _loggerMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IRepository<User, UserId>>();
        _userDomainServiceMock = new Mock<IUserDomainService>();
        _usersCacheServiceMock = new Mock<IUsersCacheService>();
        _dateTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _loggerMock = new Mock<ILogger<DeleteUserCommandHandler>>();

        _uowMock
            .Setup(x => x.GetRepository<User, UserId>())
            .Returns(_userRepositoryMock.Object);

        _handler = new DeleteUserCommandHandler(
            _uowMock.Object,
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

        _uowMock
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

        _uowMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _usersCacheServiceMock.Verify(
            x => x.InvalidateUserAsync(userId, It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}