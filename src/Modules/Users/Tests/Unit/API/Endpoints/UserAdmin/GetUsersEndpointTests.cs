using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints.UserAdmin;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "API")]
[Trait("Endpoint", "GetUsers")]
public class GetUsersEndpointTests
{
    private readonly Mock<IQueryDispatcher> _mockQueryDispatcher;

    public GetUsersEndpointTests()
    {
        _mockQueryDispatcher = new Mock<IQueryDispatcher>();
    }

    [Fact]
    public async Task GetUsersAsync_WithDefaultParameters_ShouldReturnPagedUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            CreateUserDto("user1@test.com", "user1"),
            CreateUserDto("user2@test.com", "user2")
        };

        var pagedResult = new PagedResult<UserDto>(users, 1, 10, 2);
        var successResult = Result<PagedResult<UserDto>>.Success(pagedResult);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeEndpoint();

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.Is<GetUsersQuery>(q =>
                q.Page == 1 &&
                q.PageSize == 10 &&
                q.SearchTerm == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WithCustomPagination_ShouldUseCorrectParameters()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 20;
        var users = new List<UserDto>();

        var pagedResult = new PagedResult<UserDto>(users, pageNumber, pageSize, 0);
        var successResult = Result<PagedResult<UserDto>>.Success(pagedResult);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeEndpoint(pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.Is<GetUsersQuery>(q =>
                q.Page == pageNumber &&
                q.PageSize == pageSize &&
                q.SearchTerm == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WithSearchTerm_ShouldFilterUsers()
    {
        // Arrange
        var searchTerm = "john";
        var users = new List<UserDto>
        {
            CreateUserDto("john@test.com", "john_doe")
        };

        var pagedResult = new PagedResult<UserDto>(users, 1, 10, 1);
        var successResult = Result<PagedResult<UserDto>>.Success(pagedResult);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeEndpoint(searchTerm: searchTerm);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.Is<GetUsersQuery>(q =>
                q.Page == 1 &&
                q.PageSize == 10 &&
                q.SearchTerm == searchTerm),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WithAllParameters_ShouldUseAllCorrectly()
    {
        // Arrange
        var pageNumber = 3;
        var pageSize = 15;
        var searchTerm = "admin";
        var users = new List<UserDto>();

        var pagedResult = new PagedResult<UserDto>(users, pageNumber, pageSize, 0);
        var successResult = Result<PagedResult<UserDto>>.Success(pagedResult);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeEndpoint(pageNumber, pageSize, searchTerm);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.Is<GetUsersQuery>(q =>
                q.Page == pageNumber &&
                q.PageSize == pageSize &&
                q.SearchTerm == searchTerm),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WithEmptySearchTerm_ShouldTreatAsEmpty()
    {
        // Arrange
        var searchTerm = string.Empty;
        var users = new List<UserDto>();

        var pagedResult = new PagedResult<UserDto>(users, 1, 10, 0);
        var successResult = Result<PagedResult<UserDto>>.Success(pagedResult);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeEndpoint(searchTerm: searchTerm);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.Is<GetUsersQuery>(q => q.SearchTerm == searchTerm),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WhenQueryFails_ShouldReturnError()
    {
        // Arrange
        var failureResult = Result<PagedResult<UserDto>>.Failure(Error.BadRequest(
            "Failed to retrieve users"));

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await InvokeEndpoint();

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.IsAny<GetUsersQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WithCancellationToken_ShouldPassTokenToDispatcher()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            InvokeEndpoint(cancellationToken: cancellationToken));

        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.IsAny<GetUsersQuery>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_WhenQueryDispatcherThrows_ShouldPropagateException()
    {
        // Arrange
        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            InvokeEndpoint());

        exception.Message.Should().Be("Database connection failed");
    }

    [Fact]
    public async Task GetUsersAsync_WithSpecialCharactersInSearchTerm_ShouldHandleCorrectly()
    {
        // Arrange
        var searchTerm = "user@domain.com";
        var users = new List<UserDto>();

        var pagedResult = new PagedResult<UserDto>(users, 1, 10, 0);
        var successResult = Result<PagedResult<UserDto>>.Success(pagedResult);

        _mockQueryDispatcher
            .Setup(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
                It.IsAny<GetUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await InvokeEndpoint(searchTerm: searchTerm);

        // Assert
        result.Should().NotBeNull();
        _mockQueryDispatcher.Verify(x => x.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            It.Is<GetUsersQuery>(q => q.SearchTerm == searchTerm),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private UserDto CreateUserDto(string email, string username)
    {
        return new UserDto(
            Id: Guid.NewGuid(),
            Username: username,
            Email: email,
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User",
            KeycloakId: Guid.NewGuid().ToString(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }

    private async Task<IResult> InvokeEndpoint(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        // Simula a chamada do endpoint através de reflexão
        var method = typeof(GetUsersEndpoint)
            .GetMethod("GetUsersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull("GetUsersAsync method should exist");

        var task = (Task<IResult>)method!.Invoke(null, new object?[]
        {
            pageNumber,
            pageSize,
            searchTerm,
            _mockQueryDispatcher.Object,
            cancellationToken
        })!;

        return await task;
    }
}