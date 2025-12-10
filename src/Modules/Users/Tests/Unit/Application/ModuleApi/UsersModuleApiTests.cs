using FluentAssertions;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.ModuleApi;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.ModuleApi;

/// <summary>
/// Testes unitários para UsersModuleApi
/// Valida mapeamento de DTOs internos para DTOs de módulo
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Component", "ModuleApi")]
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
    public async Task IsAvailableAsync_WhenHealthy_ShouldReturnTrue()
    {
        // Arrange
        _serviceProvider
            .Setup(sp => sp.GetService(typeof(HealthCheckService)))
            .Returns(default(HealthCheckService));

        // Mock getUserByIdHandler for CanExecuteBasicOperationsAsync
        _getUserByIdHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(null!));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenUnhealthy_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckService = new Mock<HealthCheckService>();
        healthCheckService
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.Zero));

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckService.Object);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthCheckServiceNotFound_ShouldReturnFalse()
    {
        // Arrange
        _serviceProvider
            .Setup(sp => sp.GetService(typeof(HealthCheckService)))
            .Returns((HealthCheckService?)null);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckService = new Mock<HealthCheckService>();
        healthCheckService
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Health check failed"));

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckService.Object);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private const int TestUsernameLength = 10;

    private static UserDto CreateUserDto(Guid id, string? email = null)
    {
        return new UserDto(
            Id: id,
            Username: $"user_{id:N}".Substring(0, TestUsernameLength),
            Email: email ?? $"user_{id:N}@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: Guid.NewGuid().ToString(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null);
    }

    #endregion
}
