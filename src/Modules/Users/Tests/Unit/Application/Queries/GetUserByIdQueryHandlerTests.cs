using MeAjudaAi.Modules.Users.Application.Caching;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUsersCacheService> _usersCacheServiceMock;
    private readonly Mock<ILogger<GetUserByIdQueryHandler>> _loggerMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _usersCacheServiceMock = new Mock<IUsersCacheService>();
        _loggerMock = new Mock<ILogger<GetUserByIdQueryHandler>>();
        _handler = new GetUserByIdQueryHandler(
            _userRepositoryMock.Object,
            _usersCacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_ShouldReturnUserSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var userDto = new UserDto(
            userId,
            "testuser",
            "test@example.com",
            "Test",
            "User",
            "Test User",
            "keycloak-id-123",
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);

        _usersCacheServiceMock.Verify(
            x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().NotBeNullOrEmpty();

        _usersCacheServiceMock.Verify(
            x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyGuid_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.Empty);

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                Guid.Empty,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("User not found");

        _usersCacheServiceMock.Verify(
            x => x.GetOrCacheUserByIdAsync(
                Guid.Empty,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CacheServiceThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _usersCacheServiceMock.Verify(
            x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_ShouldUseCorrectCacheKey()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var user = new UserBuilder()
            .WithId(userId)
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .WithFirstName("Test")
            .WithLastName("User")
            .Build();
        var userDto = user.ToDto();

        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _usersCacheServiceMock.Verify(
            x => x.GetOrCacheUserByIdAsync(
                userId, // Verifica se o userId correto foi passado
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CacheMiss_ShouldCallRepositoryAndReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .WithFirstName("Test")
            .WithLastName("User")
            .Build();

        // Configura o serviço de cache para chamar a função de fábrica (simulando cache miss)
        _usersCacheServiceMock
            .Setup(x => x.GetOrCacheUserByIdAsync(
                userId,
                It.IsAny<Func<CancellationToken, ValueTask<UserDto?>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Guid, Func<CancellationToken, ValueTask<UserDto?>>, CancellationToken>(
                async (id, factory, ct) => await factory(ct));

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<UserId>(uid => uid.Value == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be("testuser");
        result.Value!.Email.Should().Be("test@example.com");

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.Is<UserId>(uid => uid.Value == userId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
