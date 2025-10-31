using FluentAssertions;
using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints.UserAdmin;

[Trait("Category", "Unit")]
public class GetUserByEmailEndpointTests
{
    private readonly Mock<IQueryDispatcher> _mockQueryDispatcher;

    public GetUserByEmailEndpointTests()
    {
        _mockQueryDispatcher = new Mock<IQueryDispatcher>();
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(
            userId,
            "testuser",
            email,
            "Test",
            "User",
            "Test User",
            "EN",
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        var expectedResult = Result<UserDto>.Success(expectedUser);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.Is<GetUserByEmailQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.Is<GetUserByEmailQuery>(q => q.Email == email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var expectedResult = Result<UserDto>.Failure(Error.NotFound("User not found"));

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.Is<GetUserByEmailQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.Is<GetUserByEmailQuery>(q => q.Email == email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithEmptyEmail_ShouldProcessQuery()
    {
        // Arrange
        var email = "";
        var expectedResult = Result<UserDto>.Failure(Error.BadRequest("Email cannot be empty"));

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.Is<GetUserByEmailQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.Is<GetUserByEmailQuery>(q => q.Email == email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithCancellation_ShouldPassCancellationToken()
    {
        // Arrange
        var email = "test@example.com";
        var cancellationToken = new CancellationToken(true);
        var expectedResult = Result<UserDto>.Failure(Error.Internal("Operation was cancelled"));

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.IsAny<GetUserByEmailQuery>(),
                cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.IsAny<GetUserByEmailQuery>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithSpecialCharactersInEmail_ShouldProcessQuery()
    {
        // Arrange
        var email = "test+tag@example-domain.co.uk";
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(
            userId,
            "testuser",
            email,
            "Test",
            "User",
            "Test User",
            "EN",
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        var expectedResult = Result<UserDto>.Success(expectedUser);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.Is<GetUserByEmailQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.Is<GetUserByEmailQuery>(q => q.Email == email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithUppercaseEmail_ShouldProcessQuery()
    {
        // Arrange
        var email = "TEST@EXAMPLE.COM";
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(
            userId,
            "testuser",
            email.ToLowerInvariant(),
            "Test",
            "User",
            "Test User",
            "EN",
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        var expectedResult = Result<UserDto>.Success(expectedUser);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.Is<GetUserByEmailQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.Is<GetUserByEmailQuery>(q => q.Email == email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithQueryDispatcherException_ShouldPropagateException()
    {
        // Arrange
        var email = "test@example.com";
        var expectedException = new InvalidOperationException("Database connection failed");

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.IsAny<GetUserByEmailQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None));

        exception.Should().Be(expectedException);
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.IsAny<GetUserByEmailQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithMultipleCallsSameEmail_ShouldProcessAllCalls()
    {
        // Arrange
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(
            userId,
            "testuser",
            email,
            "Test",
            "User",
            "Test User",
            "EN",
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        var expectedResult = Result<UserDto>.Success(expectedUser);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
                It.Is<GetUserByEmailQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result1 = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None);
        var result2 = await InvokeEndpointMethod(email, _mockQueryDispatcher.Object, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            It.Is<GetUserByEmailQuery>(q => q.Email == email),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static async Task<IResult> InvokeEndpointMethod(
        string email,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        // Use reflection to call the private static method
        var method = typeof(GetUserByEmailEndpoint).GetMethod(
            "GetUserByEmailAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var task = (Task<IResult>)method!.Invoke(null, new object[] { email, queryDispatcher, cancellationToken })!;
        return await task;
    }
}