using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUserByUsernameQueryHandlerTests
{
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<ILogger<GetUserByUsernameQueryHandler>> _loggerMock;
    private readonly GetUserByUsernameQueryHandler _handler;

    public GetUserByUsernameQueryHandlerTests()
    {
        _userQueriesMock = new Mock<IUserQueries>();
        _loggerMock = new Mock<ILogger<GetUserByUsernameQueryHandler>>();
        _handler = new GetUserByUsernameQueryHandler(_userQueriesMock.Object, _loggerMock.Object);
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

        _userQueriesMock
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

        _userQueriesMock.Verify(
            x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var username = "nonexistentuser";
        var query = new GetUserByUsernameQuery(username);

        _userQueriesMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be(ValidationMessages.NotFound.User);

        _userQueriesMock.Verify(
            x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var username = "testuser";
        var query = new GetUserByUsernameQuery(username);
        var exception = new InvalidOperationException("Database connection failed");

        _userQueriesMock
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

        _userQueriesMock.Verify(
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

        _userQueriesMock
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
        await cancellationTokenSource.CancelAsync();

        _userQueriesMock
            .Setup(x => x.GetByUsernameAsync(It.Is<Username>(u => u.Value == username), It.Is<CancellationToken>(t => t == cancellationTokenSource.Token)))
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
