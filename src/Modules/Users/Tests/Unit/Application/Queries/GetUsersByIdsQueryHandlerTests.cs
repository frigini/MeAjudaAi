using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUsersByIdsQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUsersCacheService> _usersCacheServiceMock;
    private readonly Mock<ILogger<GetUsersByIdsQueryHandler>> _loggerMock;
    private readonly GetUsersByIdsQueryHandler _handler;

    public GetUsersByIdsQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _usersCacheServiceMock = new Mock<IUsersCacheService>();
        _loggerMock = new Mock<ILogger<GetUsersByIdsQueryHandler>>();
        _handler = new GetUsersByIdsQueryHandler(
            _userRepositoryMock.Object,
            _usersCacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleValidIds_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = new UserBuilder().WithUsername("user1").WithEmail("user1@test.com").Build();
        var user2 = new UserBuilder().WithUsername("user2").WithEmail("user2@test.com").Build();
        var user3 = new UserBuilder().WithUsername("user3").WithEmail("user3@test.com").Build();

        var userIds = new List<Guid> { user1.Id.Value, user2.Id.Value, user3.Id.Value };
        var userIdVOs = userIds.Select(id => new UserId(id)).ToList();
        var users = new List<User> { user1, user2, user3 };

        var query = new GetUsersByIdsQuery(userIds);

        _userRepositoryMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, Func<CancellationToken, ValueTask<UserDto?>> factory, CancellationToken ct) => factory(ct).Result);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value.Select(u => u.Id).Should().Contain(userIds);

        _userRepositoryMock.Verify(
            x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyIdsList_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetUsersByIdsQuery(new List<Guid>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();

        _userRepositoryMock.Verify(
            x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSomeUsersNotFound_ShouldReturnOnlyFoundUsers()
    {
        // Arrange
        var user1 = new UserBuilder().WithUsername("user1").WithEmail("user1@test.com").Build();
        var user2 = new UserBuilder().WithUsername("user2").WithEmail("user2@test.com").Build();

        var requestedIds = new List<Guid> { user1.Id.Value, user2.Id.Value, Guid.NewGuid() };
        var foundUsers = new List<User> { user1, user2 };

        var query = new GetUsersByIdsQuery(requestedIds);

        _userRepositoryMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(foundUsers);

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, Func<CancellationToken, ValueTask<UserDto?>> factory, CancellationToken ct) => factory(ct).Result);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoUsersFound_ShouldReturnEmptyList()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new GetUsersByIdsQuery(userIds);

        _userRepositoryMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new GetUsersByIdsQuery(userIds);

        _userRepositoryMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Failed to retrieve users in batch");
    }

    [Fact]
    public async Task HandleAsync_WithSingleId_ShouldReturnSingleUser()
    {
        // Arrange
        var user = new UserBuilder().WithUsername("singleuser").WithEmail("single@test.com").Build();
        var userIds = new List<Guid> { user.Id.Value };
        var users = new List<User> { user };

        var query = new GetUsersByIdsQuery(userIds);

        _userRepositoryMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, Func<CancellationToken, ValueTask<UserDto?>> factory, CancellationToken ct) => factory(ct).Result);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(user.Id.Value);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateIds_ShouldHandleCorrectly()
    {
        // Arrange
        var user1 = new UserBuilder().WithUsername("user1").WithEmail("user1@test.com").Build();
        var user2 = new UserBuilder().WithUsername("user2").WithEmail("user2@test.com").Build();

        var userIds = new List<Guid> { user1.Id.Value, user2.Id.Value, user1.Id.Value };
        var users = new List<User> { user1, user2 };

        var query = new GetUsersByIdsQuery(userIds);

        _userRepositoryMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IReadOnlyList<UserId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, Func<CancellationToken, ValueTask<UserDto?>> factory, CancellationToken ct) => factory(ct).Result);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }
}
