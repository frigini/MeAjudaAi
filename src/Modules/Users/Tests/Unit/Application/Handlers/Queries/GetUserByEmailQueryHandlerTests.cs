using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<ILogger<GetUserByEmailQueryHandler>> _loggerMock;
    private readonly GetUserByEmailQueryHandler _handler;

    public GetUserByEmailQueryHandlerTests()
    {
        _userQueriesMock = new Mock<IUserQueries>();
        _loggerMock = new Mock<ILogger<GetUserByEmailQueryHandler>>();
        _handler = new GetUserByEmailQueryHandler(_userQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_ShouldReturnUserSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var query = new GetUserByEmailQuery(email);
        var user = new UserBuilder()
            .WithEmail(email)
            .WithUsername("testuser")
            .WithFirstName("Test")
            .WithLastName("User")
            .Build();

        _userQueriesMock
            .Setup(x => x.GetByEmailAsync(It.Is<Email>(e => e.Value == email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(email);

        _userQueriesMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var query = new GetUserByEmailQuery(email);

        _userQueriesMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().NotBeNullOrEmpty();

        _userQueriesMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_EmptyOrNullEmail_ShouldReturnFailure(string? invalidEmail)
    {
        // Arrange
        var query = new GetUserByEmailQuery(invalidEmail ?? string.Empty);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _userQueriesMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmailFormat_ShouldReturnFailure()
    {
        // Arrange
        var invalidEmail = "invalid-email-format";
        var query = new GetUserByEmailQuery(invalidEmail);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _userQueriesMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var query = new GetUserByEmailQuery(email);

        _userQueriesMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _userQueriesMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_LongEmail_ShouldReturnFailure()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com";
        var query = new GetUserByEmailQuery(longEmail);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        _userQueriesMock.Verify(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
