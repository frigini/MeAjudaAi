using FluentAssertions;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Services;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Time;
using NSubstitute;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.ModuleApi;

public class UsersModuleApiTests
{
    private readonly IQueryHandler<GetUserByIdQuery, Result<UserDto>> _getUserByIdHandler;
    private readonly IQueryHandler<GetUserByEmailQuery, Result<UserDto>> _getUserByEmailHandler;
    private readonly UsersModuleApi _sut;

    public UsersModuleApiTests()
    {
        _getUserByIdHandler = Substitute.For<IQueryHandler<GetUserByIdQuery, Result<UserDto>>>();
        _getUserByEmailHandler = Substitute.For<IQueryHandler<GetUserByEmailQuery, Result<UserDto>>>();
        _sut = new UsersModuleApi(_getUserByIdHandler, _getUserByEmailHandler);
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
    public async Task IsAvailableAsync_ShouldReturn_True()
    {
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
            .HandleAsync(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(userDto));

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
            .HandleAsync(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(null));

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
            .HandleAsync(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Failure(error));

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
            .HandleAsync(Arg.Any<GetUserByEmailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(userDto));

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

        _getUserByIdHandler
            .HandleAsync(Arg.Is<GetUserByIdQuery>(q => q.UserId == userId1), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(userDto1));
        
        _getUserByIdHandler
            .HandleAsync(Arg.Is<GetUserByIdQuery>(q => q.UserId == userId2), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(userDto2));

        // Act
        var result = await _sut.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(u => u.Id == userId1 && u.Username == "user1");
        result.Value.Should().Contain(u => u.Id == userId2 && u.Username == "user2");
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var userDto = new UserDto(userId, "test", "test@test.com", "Test", "User", "Test User", UuidGenerator.NewIdString(), DateTime.UtcNow, null);

        _getUserByIdHandler
            .HandleAsync(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(userDto));

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
            .HandleAsync(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(null));

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
            .HandleAsync(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Failure("Database error"));

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
            .HandleAsync(Arg.Any<GetUserByEmailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(userDto));

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
            .HandleAsync(Arg.Any<GetUserByEmailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(null));

        // Act
        var result = await _sut.EmailExistsAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnFalse_AsNotImplemented()
    {
        // Arrange
        var username = "testuser";

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
            .HandleAsync(Arg.Any<GetUserByEmailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserDto>.Success(null));

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        await _getUserByEmailHandler
            .Received(1)
            .HandleAsync(Arg.Is<GetUserByEmailQuery>(q => q.Email == email), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUsersBatchAsync_WithEmptyList_ShouldReturnEmptyResult()
    {
        // Arrange
        var emptyIds = new List<Guid>();

        // Act
        var result = await _sut.GetUsersBatchAsync(emptyIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}