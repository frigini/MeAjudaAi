using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Handlers.Queries;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetUsersQueryHandler>> _loggerMock;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetUsersQueryHandler>>();
        _handler = new GetUsersQueryHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidPaginationParameters_ShouldReturnSuccessWithData()
    {
        // Arrange
        var query = new GetUsersQuery(Page: 1, PageSize: 10, SearchTerm: null);
        var users = CreateTestUsers(5);
        var totalCount = 25;

        _userRepositoryMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, totalCount));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        var pagedResult = result.Value;
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().HaveCount(5);
        pagedResult.TotalCount.Should().Be(totalCount);
        pagedResult.Page.Should().Be(query.Page);
        pagedResult.PageSize.Should().Be(query.PageSize);
        pagedResult.TotalPages.Should().Be(3); // 25 / 10 = 3 pages

        _userRepositoryMock.Verify(
            x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyResult_ShouldReturnSuccessWithEmptyList()
    {
        // Arrange
        var query = new GetUsersQuery(Page: 1, PageSize: 10, SearchTerm: null);
        var users = new List<User>();
        var totalCount = 0;

        _userRepositoryMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, totalCount));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        var pagedResult = result.Value;
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().BeEmpty();
        pagedResult.TotalCount.Should().Be(0);
        pagedResult.Page.Should().Be(query.Page);
        pagedResult.PageSize.Should().Be(query.PageSize);
        pagedResult.TotalPages.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task HandleAsync_InvalidPaginationParameters_ShouldReturnFailure(int page, int pageSize)
    {
        // Arrange
        var query = new GetUsersQuery(Page: page, PageSize: pageSize, SearchTerm: null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Invalid pagination parameters");

        _userRepositoryMock.Verify(
            x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetUsersQuery(Page: 1, PageSize: 10, SearchTerm: null);
        var exceptionMessage = "Database connection failed";

        _userRepositoryMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_LargePageSize_ShouldStillWork()
    {
        // Arrange
        var query = new GetUsersQuery(Page: 1, PageSize: 100, SearchTerm: null); // Max allowed
        var users = CreateTestUsers(50);
        var totalCount = 150;

        _userRepositoryMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, totalCount));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        var pagedResult = result.Value;
        pagedResult.Items.Should().HaveCount(50);
        pagedResult.TotalCount.Should().Be(totalCount);
        pagedResult.TotalPages.Should().Be(2); // 150 / 100 = 2 pages
    }

    [Fact]
    public async Task HandleAsync_WithSearchTerm_ShouldPassToRepository()
    {
        // Arrange
        var searchTerm = "john";
        var query = new GetUsersQuery(Page: 1, PageSize: 10, SearchTerm: searchTerm);
        var users = CreateTestUsers(3);
        var totalCount = 3;

        _userRepositoryMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, totalCount));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        _userRepositoryMock.Verify(
            x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CancellationRequested_ShouldPassCancellationToken()
    {
        // Arrange
        var query = new GetUsersQuery(Page: 1, PageSize: 10, SearchTerm: null);
        var cancellationToken = new CancellationToken(true);

        _userRepositoryMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapUsersToDto_Correctly()
    {
        // Arrange
        var query = new GetUsersQuery(Page: 1, PageSize: 10, SearchTerm: null);
        var user = CreateTestUser("testuser", "test@example.com", "John", "Doe");
        var users = new List<User> { user };
        var totalCount = 1;

        _userRepositoryMock
            .Setup(x => x.GetPagedAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, totalCount));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        var pagedResult = result.Value;
        var userDto = pagedResult.Items.First();
        
        userDto.Id.Should().Be(user.Id);
        userDto.Username.Should().Be(user.Username.Value);
        userDto.Email.Should().Be(user.Email.Value);
        userDto.FirstName.Should().Be(user.FirstName);
        userDto.LastName.Should().Be(user.LastName);
        userDto.FullName.Should().Be($"{user.FirstName} {user.LastName}");
        userDto.CreatedAt.Should().Be(user.CreatedAt);
        userDto.UpdatedAt.Should().Be(user.UpdatedAt);
    }

    private static List<User> CreateTestUsers(int count)
    {
        var users = new List<User>();
        for (int i = 1; i <= count; i++)
        {
            users.Add(CreateTestUser($"user{i}", $"user{i}@example.com", $"First{i}", $"Last{i}"));
        }
        return users;
    }

    private static User CreateTestUser(string username, string email, string firstName, string lastName)
    {
        return new User(
            username: new Username(username),
            email: new Email(email),
            firstName: firstName,
            lastName: lastName,
            keycloakId: Guid.NewGuid().ToString()
        );
    }
}