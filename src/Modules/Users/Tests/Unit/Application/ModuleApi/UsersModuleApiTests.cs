using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.ModuleApi;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Component", "ModuleApi")]
public class UsersModuleApiTests
{
    private readonly Mock<IQueryHandler<GetUserByIdQuery, Result<UserDto>>> _getUserByIdHandler;
    private readonly Mock<IQueryHandler<GetUserByEmailQuery, Result<UserDto>>> _getUserByEmailHandler;
    private readonly Mock<IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>> _getUserByUsernameHandler;
    private readonly Mock<IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>>> _getUsersByIdsHandler;
    private readonly Mock<IUserQueries> _userQueries;
    private readonly UsersModuleApi _sut;

    public UsersModuleApiTests()
    {
        _getUserByIdHandler = new Mock<IQueryHandler<GetUserByIdQuery, Result<UserDto>>>();
        _getUserByEmailHandler = new Mock<IQueryHandler<GetUserByEmailQuery, Result<UserDto>>>();
        _getUserByUsernameHandler = new Mock<IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>>();
        _getUsersByIdsHandler = new Mock<IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>>>();
        _userQueries = new Mock<IUserQueries>();

        _userQueries.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _sut = new UsersModuleApi(
            _getUserByIdHandler.Object,
            _getUserByEmailHandler.Object,
            _getUserByUsernameHandler.Object,
            _getUsersByIdsHandler.Object,
            _userQueries.Object);
    }

    #region Module Metadata

    [Fact]
    public void ModuleName_ShouldReturnUsers()
    {
        // Act
        var result = _sut.ModuleName;

        // Assert
        result.Should().Be("Users");
    }

    [Fact]
    public void ApiVersion_ShouldReturn1Point0()
    {
        // Act
        var result = _sut.ApiVersion;

        // Assert
        result.Should().Be("1.0");
    }

    #endregion

    #region GetUserByIdAsync

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnMappedModuleDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = CreateUserDto(userId);

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.Is<GetUserByIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Username.Should().Be(userDto.Username);
        result.Value.Email.Should().Be(userDto.Email);
        result.Value.FirstName.Should().Be(userDto.FirstName);
        result.Value.LastName.Should().Be(userDto.LastName);
        result.Value.FullName.Should().Be(userDto.FullName);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenHandlerFails_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.Internal("Database error");

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(error));

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await _sut.GetUserByIdAsync(userId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region GetUserByEmailAsync

    [Fact]
    public async Task GetUserByEmailAsync_WithExistingUser_ShouldReturnMappedModuleDto()
    {
        // Arrange
        var email = "test@example.com";
        var userDto = CreateUserDto(Guid.NewGuid(), email);

        _getUserByEmailHandler
            .Setup(h => h.HandleAsync(It.Is<GetUserByEmailQuery>(q => q.Email == email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(email);
        result.Value.Username.Should().Be(userDto.Username);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _getUserByEmailHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion

    #region GetUsersBatchAsync

    [Fact]
    public async Task GetUsersBatchAsync_WithMultipleUsers_ShouldReturnMappedBasicDtos()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userIds = new List<Guid> { userId1, userId2 };

        var userDtos = new List<UserDto>
        {
            CreateUserDto(userId1, "user1@example.com"),
            CreateUserDto(userId2, "user2@example.com")
        };

        _getUsersByIdsHandler
            .Setup(h => h.HandleAsync(
                It.Is<GetUsersByIdsQuery>(q => q.UserIds.SequenceEqual(userIds)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<UserDto>>.Success(userDtos));

        // Act
        var result = await _sut.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be(userId1);
        result.Value[0].Email.Should().Be("user1@example.com");
        result.Value[0].IsActive.Should().BeTrue();
        result.Value[1].Id.Should().Be(userId2);
        result.Value[1].Email.Should().Be("user2@example.com");
    }

    [Fact]
    public async Task GetUsersBatchAsync_WithEmptyList_ShouldReturnEmptyResult()
    {
        // Arrange
        var userIds = new List<Guid>();

        _getUsersByIdsHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUsersByIdsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<UserDto>>.Success(new List<UserDto>()));

        // Act
        var result = await _sut.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        _getUsersByIdsHandler.Verify(
            h => h.HandleAsync(It.IsAny<GetUsersByIdsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Handler should be called once even for empty input (short-circuits inside handler)");
    }

    #endregion

    #region UserExistsAsync

    [Fact]
    public async Task UserExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = CreateUserDto(userId);

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.UserExistsAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(Error.NotFound("User not found")));

        // Act
        var result = await _sut.UserExistsAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region EmailExistsAsync

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "existing@example.com";
        var userDto = CreateUserDto(Guid.NewGuid(), email);

        _getUserByEmailHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.EmailExistsAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _getUserByEmailHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(Error.NotFound("User not found")));

        // Act
        var result = await _sut.EmailExistsAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region IsAvailableAsync

    [Fact]
    public async Task IsAvailableAsync_WhenNoHealthCheckServiceAndBasicOperationsSucceed_ShouldReturnTrue()
    {
        // Arrange
        _userQueries.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthCheckServiceReturnsHealthy_ShouldReturnTrue()
    {
        // Arrange
        _userQueries.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeTrue();
        _userQueries.Verify(x => x.CanConnectAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthCheckServiceReturnsUnhealthy_ShouldReturnFalse()
    {
        // Arrange
        _userQueries.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenNoHealthCheckServiceAndBasicOperationsFail_ShouldReturnFalse()
    {
        // Arrange
        _userQueries.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthCheckServiceThrowsException_ShouldReturnFalse()
    {
        // Arrange
        _userQueries.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Health check failed"));

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static UserDto CreateUserDto(Guid id, string? email = null)
    {
        return new UserDto(
            Id: id,
            Username: $"user_{id:N}",
            Email: email ?? $"user_{id:N}@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            DeviceToken: null,
            KeycloakId: Guid.NewGuid().ToString(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null);
    }

    #endregion
}
