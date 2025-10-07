using FluentAssertions;
using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints.UserAdmin;

[Trait("Category", "Unit")]
public class GetUserByIdEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;

    public GetUserByIdEndpointTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
    }

    [Fact]
    public void GetUserByIdEndpoint_ShouldInheritFromBaseEndpoint()
    {
        // Arrange & Act
        var endpointType = typeof(GetUserByIdEndpoint);

        // Assert
        endpointType.BaseType?.Name.Should().Be("BaseEndpoint");
    }

    [Fact]
    public void GetUserByIdEndpoint_ShouldImplementIEndpoint()
    {
        // Arrange & Act
        var endpointType = typeof(GetUserByIdEndpoint);

        // Assert
        endpointType.GetInterface("IEndpoint").Should().NotBeNull();
    }

    [Fact]
    public void Map_ShouldBeStaticMethod()
    {
        // Arrange
        var mapMethod = typeof(GetUserByIdEndpoint).GetMethod("Map", BindingFlags.Public | BindingFlags.Static);

        // Assert
        mapMethod.Should().NotBeNull();
        mapMethod!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserAsync_WithValidId_ShouldReturnOkWithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var userDto = new UserDto(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: "keycloak-123",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );
        var successResult = Result<UserDto>.Success(userDto);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.Is<GetUserByIdQuery>(q => q.UserId == userId),
                cancellationToken))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeGetUserAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var httpResult = result as IStatusCodeHttpResult;
        httpResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        _queryDispatcherMock.Verify(
            x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.Is<GetUserByIdQuery>(q => q.UserId == userId),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUserAsync_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var notFoundResult = Error.NotFound("User not found");

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.Is<GetUserByIdQuery>(q => q.UserId == userId),
                cancellationToken))
            .ReturnsAsync(notFoundResult);

        // Act
        var result = await InvokeGetUserAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var httpResult = result as IStatusCodeHttpResult;
        httpResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetUserAsync_WithInternalError_ShouldReturnInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var internalError = Error.Internal("Internal server error");

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.Is<GetUserByIdQuery>(q => q.UserId == userId),
                cancellationToken))
            .ReturnsAsync(internalError);

        // Act
        var result = await InvokeGetUserAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        var httpResult = result as IStatusCodeHttpResult;
        httpResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task GetUserAsync_WithCancellationToken_ShouldPassTokenToDispatcher()
    {
        // Arrange
        var userId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var userDto = new UserDto(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: "keycloak-123",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );
        var successResult = Result<UserDto>.Success(userDto);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.IsAny<GetUserByIdQuery>(),
                cancellationToken))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeGetUserAsync(userId, cancellationToken);

        // Assert
        _queryDispatcherMock.Verify(
            x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.IsAny<GetUserByIdQuery>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public void ToQuery_WithValidGuid_ShouldCreateCorrectQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var query = userId.ToQuery();

        // Assert
        query.Should().NotBeNull();
        query.UserId.Should().Be(userId);
        query.Should().BeOfType<GetUserByIdQuery>();
    }

    [Fact]
    public void ToQuery_WithEmptyGuid_ShouldCreateQueryWithEmptyGuid()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var query = userId.ToQuery();

        // Assert
        query.Should().NotBeNull();
        query.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ToQuery_ShouldAlwaysCreateNewInstance()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var query1 = userId.ToQuery();
        var query2 = userId.ToQuery();

        // Assert
        query1.Should().NotBeSameAs(query2);
        query1.Should().BeEquivalentTo(query2, options => options.Excluding(x => x.CorrelationId));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-5678-9012-123456789012")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public void ToQuery_WithDifferentGuids_ShouldCreateCorrectQueries(string guidString)
    {
        // Arrange
        var userId = Guid.Parse(guidString);

        // Act
        var query = userId.ToQuery();

        // Assert
        query.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserAsync_WithValidUserId_ShouldMapIdCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var userDto = new UserDto(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: "keycloak-123",
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow
        );
        var successResult = Result<UserDto>.Success(userDto);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.Is<GetUserByIdQuery>(q => q.UserId == userId),
                cancellationToken))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeGetUserAsync(userId, cancellationToken);

        // Assert
        _queryDispatcherMock.Verify(
            x => x.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
                It.Is<GetUserByIdQuery>(q => q.UserId == userId),
                cancellationToken),
            Times.Once);
    }

    private async Task<IResult> InvokeGetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var getUserAsyncMethod = typeof(GetUserByIdEndpoint)
            .GetMethod("GetUserAsync", BindingFlags.NonPublic | BindingFlags.Static);

        getUserAsyncMethod.Should().NotBeNull("GetUserAsync method should exist");

        var task = (Task<IResult>)getUserAsyncMethod!.Invoke(null, [id, _queryDispatcherMock.Object, cancellationToken])!;
        return await task;
    }
}