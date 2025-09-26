using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUserByUsernameQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetUserByUsernameQueryHandler>> _loggerMock;
    private readonly GetUserByUsernameQueryHandler _handler;

    public GetUserByUsernameQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetUserByUsernameQueryHandler>>();
        _handler = new GetUserByUsernameQueryHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_ShouldReturnUserSuccessfully()
    {
        // Arrange
        var username = "testuser";
        var query = new GetUserByUsernameQuery(username);
        var user = new UserBuilder()
            .WithUsername(username)
            .WithEmail("test@example.com")
            .WithFirstName("Test")
            .WithLastName("User")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be(username);
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FirstName.Should().Be("Test");
        result.Value.LastName.Should().Be("User");
        
        _userRepositoryMock.Verify(
            x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var username = "nonexistentuser";
        var query = new GetUserByUsernameQuery(username);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("User not found");
        
        _userRepositoryMock.Verify(
            x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var username = "testuser";
        var query = new GetUserByUsernameQuery(username);
        var exception = new Exception("Database connection failed");

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Failed to retrieve user");
        result.Error.Message.Should().Contain("Database connection failed");
        
        _userRepositoryMock.Verify(
            x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyUsername_ShouldReturnFailure()
    {
        // Arrange
        var emptyUsername = "";
        var query = new GetUserByUsernameQuery(emptyUsername);

        // Act & Assert - Username value object vai lançar exceção que será capturada pelo handler
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Failed to retrieve user");
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("test_user")]
    public async Task HandleAsync_ValidUsernames_ShouldProcessCorrectly(string username)
    {
        // Arrange
        var query = new GetUserByUsernameQuery(username);
        var user = new UserBuilder()
            .WithUsername(username)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be(username);
    }

    [Fact]
    public async Task HandleAsync_CancellationRequested_ShouldReturnFailure()
    {
        // Arrange
        var username = "testuser";
        var query = new GetUserByUsernameQuery(username);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _handler.HandleAsync(query, cancellationTokenSource.Token);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Failed to retrieve user");
    }
}