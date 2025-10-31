using FluentAssertions;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Services;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Services;

public class UsersModuleApiTests
{
    private readonly Mock<IQueryHandler<GetUserByIdQuery, Result<UserDto>>> _getUserByIdHandler;
    private readonly Mock<IQueryHandler<GetUserByEmailQuery, Result<UserDto>>> _getUserByEmailHandler;
    private readonly Mock<IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>> _getUserByUsernameHandler;
    private readonly Mock<IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>>> _getUsersByIdsHandler;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<ILogger<UsersModuleApi>> _logger;
    private readonly UsersModuleApi _sut;

    public UsersModuleApiTests()
    {
        _getUserByIdHandler = new Mock<IQueryHandler<GetUserByIdQuery, Result<UserDto>>>();
        _getUserByEmailHandler = new Mock<IQueryHandler<GetUserByEmailQuery, Result<UserDto>>>();
        _getUserByUsernameHandler = new Mock<IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>>();
        _getUsersByIdsHandler = new Mock<IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>>>();
        _serviceProvider = new Mock<IServiceProvider>();
        _logger = new Mock<ILogger<UsersModuleApi>>();

        _sut = new UsersModuleApi(
            _getUserByIdHandler.Object,
            _getUserByEmailHandler.Object,
            _getUserByUsernameHandler.Object,
            _getUsersByIdsHandler.Object,
            _serviceProvider.Object,
            _logger.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturn_Users()
    {
        // Act
        var result = _sut.ModuleName;

        // Assert
        result.Should().Be("Users");
    }

    [Fact]
    public void ApiVersion_ShouldReturn_Version1()
    {
        // Act
        var result = _sut.ApiVersion;

        // Assert
        result.Should().Be("1.0");
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthy_ShouldReturn_True()
    {
        // Arrange
        _getUserByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(Error.NotFound("User not found")));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
        _getUserByIdHandler.Verify(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenBasicOperationsFail_ShouldReturn_False()
    {
        // Arrange
        _getUserByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthCheckServiceUnavailable_ShouldStillCheckBasicOperations()
    {
        // Arrange
        _serviceProvider.Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns((HealthCheckService?)null);

        _getUserByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(Error.NotFound(ValidationMessages.NotFound.User)));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnModuleUserDto()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var userDto = new UserDto(
            userId,
            "testuser",
            "test@example.com",
            "John",
            "Doe",
            "John Doe",
            UuidGenerator.NewIdString(),
            DateTime.UtcNow,
            null);

        _getUserByIdHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Username.Should().Be("testuser");
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = UuidGenerator.NewId();

        _getUserByIdHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
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
        var userId = UuidGenerator.NewId();
        var error = Error.BadRequest("Database error");

        _getUserByIdHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(error));

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserExists_ShouldReturnModuleUserDto()
    {
        // Arrange
        var email = "test@example.com";
        var userDto = new UserDto(
            UuidGenerator.NewId(),
            "testuser",
            email,
            "Jane",
            "Smith",
            "Jane Smith",
            UuidGenerator.NewIdString(),
            DateTime.UtcNow,
            null);

        _getUserByEmailHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(email);
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetUsersBatchAsync_WithMultipleUsers_ShouldReturnBasicDtos()
    {
        // Arrange
        var userId1 = UuidGenerator.NewId();
        var userId2 = UuidGenerator.NewId();
        var userIds = new List<Guid> { userId1, userId2 };

        var userDto1 = new UserDto(userId1, "user1", "user1@test.com", "User", "One", "User One", UuidGenerator.NewIdString(), DateTime.UtcNow, null);
        var userDto2 = new UserDto(userId2, "user2", "user2@test.com", "User", "Two", "User Two", UuidGenerator.NewIdString(), DateTime.UtcNow, null);
        var userDtos = new List<UserDto> { userDto1, userDto2 };

        _getUsersByIdsHandler
            .Setup(x => x.HandleAsync(It.Is<GetUsersByIdsQuery>(q => q.UserIds.SequenceEqual(userIds)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<UserDto>>.Success(userDtos));

        // Act
        var result = await _sut.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(u => u.Id == userId1 && u.Username == "user1");
        result.Value.Should().Contain(u => u.Id == userId2 && u.Username == "user2");

        // Verificar que o batch handler foi chamado uma única vez
        _getUsersByIdsHandler.Verify(x => x.HandleAsync(It.IsAny<GetUsersByIdsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var userDto = new UserDto(userId, "test", "test@test.com", "Test", "User", "Test User", UuidGenerator.NewIdString(), DateTime.UtcNow, null);

        _getUserByIdHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.UserExistsAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserNotFound_ShouldReturnFalse()
    {
        // Arrange
        var userId = UuidGenerator.NewId();

        _getUserByIdHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.UserExistsAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task UserExistsAsync_WhenHandlerFails_ShouldReturnFalse()
    {
        // Arrange
        var userId = UuidGenerator.NewId();

        _getUserByIdHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure("Database error"));

        // Act
        var result = await _sut.UserExistsAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var userDto = new UserDto(UuidGenerator.NewId(), "test", email, "Test", "User", "Test User", UuidGenerator.NewIdString(), DateTime.UtcNow, null);

        _getUserByEmailHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.EmailExistsAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailNotFound_ShouldReturnFalse()
    {
        // Arrange
        var email = "notfound@example.com";

        _getUserByEmailHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.EmailExistsAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        var username = "testuser";
        _getUserByUsernameHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByUsernameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure("User not found"));

        // Act
        var result = await _sut.UsernameExistsAsync(username);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ShouldCallHandler(string email)
    {
        // Arrange
        _getUserByEmailHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        _getUserByEmailHandler
            .Verify(x => x.HandleAsync(It.Is<GetUserByEmailQuery>(q => q.Email == email), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersBatchAsync_WithEmptyList_ShouldReturnEmptyResult()
    {
        // Arrange
        var emptyIds = new List<Guid>();

        _getUsersByIdsHandler
            .Setup(x => x.HandleAsync(It.Is<GetUsersByIdsQuery>(q => q.UserIds.Count == 0), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<UserDto>>.Success(Array.Empty<UserDto>()));

        // Act
        var result = await _sut.GetUsersBatchAsync(emptyIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        // Verificar que o batch handler foi chamado
        _getUsersByIdsHandler.Verify(x => x.HandleAsync(It.IsAny<GetUsersByIdsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersBatchAsync_WhenHandlerFails_ShouldReturnFailure()
    {
        // Arrange
        var userIds = new List<Guid> { UuidGenerator.NewId() };
        var error = "Database error";

        _getUsersByIdsHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUsersByIdsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<UserDto>>.Failure(error));

        // Act
        var result = await _sut.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be(error);

        // Verificar que o batch handler foi chamado
        _getUsersByIdsHandler.Verify(x => x.HandleAsync(It.IsAny<GetUsersByIdsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UsernameExistsAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var username = "existinguser";
        var userDto = new UserDto(
            UuidGenerator.NewId(),
            username,
            "test@example.com",
            "John",
            "Doe",
            "John Doe",
            UuidGenerator.NewIdString(),
            DateTime.UtcNow,
            null);

        _getUserByUsernameHandler
            .Setup(x => x.HandleAsync(It.Is<GetUserByUsernameQuery>(q => q.Username == username), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(userDto));

        // Act
        var result = await _sut.UsernameExistsAsync(username);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _getUserByUsernameHandler
            .Verify(x => x.HandleAsync(It.Is<GetUserByUsernameQuery>(q => q.Username == username), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UsernameExistsAsync_WhenUserNotFound_ShouldReturnFalse()
    {
        // Arrange
        var username = "nonexistentuser";

        _getUserByUsernameHandler
            .Setup(x => x.HandleAsync(It.Is<GetUserByUsernameQuery>(q => q.Username == username), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure("User not found"));

        // Act
        var result = await _sut.UsernameExistsAsync(username);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();

        _getUserByUsernameHandler
            .Verify(x => x.HandleAsync(It.Is<GetUserByUsernameQuery>(q => q.Username == username), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UsernameExistsAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        var username = "testuser";
        var cancellationToken = new CancellationToken();

        _getUserByUsernameHandler
            .Setup(x => x.HandleAsync(It.IsAny<GetUserByUsernameQuery>(), cancellationToken))
            .ReturnsAsync(Result<UserDto>.Failure("User not found"));

        // Act
        var result = await _sut.UsernameExistsAsync(username, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();

        _getUserByUsernameHandler
            .Verify(x => x.HandleAsync(It.IsAny<GetUserByUsernameQuery>(), cancellationToken), Times.Once);
    }
}
