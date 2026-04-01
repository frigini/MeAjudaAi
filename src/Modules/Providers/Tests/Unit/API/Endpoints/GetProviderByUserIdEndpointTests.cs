using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class GetProviderByUserIdEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock = new();

    [Fact]
    public async Task GetProviderByUserAsync_WithValidUserId_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerDto = new ProviderDto { Id = Guid.NewGuid() };
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        var methodInfo = typeof(GetProviderByUserIdEndpoint).GetMethod("GetProviderByUserAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [userId, _queryDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<ProviderDto?>>>();
    }

    [Fact]
    public async Task GetProviderByUserAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        var methodInfo = typeof(GetProviderByUserIdEndpoint).GetMethod("GetProviderByUserAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [userId, _queryDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<NotFound<Response<object>>>();
    }

    [Fact]
    public async Task GetProviderByUserAsync_WhenQueryFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(GetProviderByUserIdEndpoint).GetMethod("GetProviderByUserAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [userId, _queryDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<ProviderDto?>>>();
    }
}
