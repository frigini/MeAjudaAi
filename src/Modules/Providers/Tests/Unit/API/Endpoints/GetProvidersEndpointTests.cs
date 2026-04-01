using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class GetProvidersEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock = new();
    private readonly Mock<ILogger<GetProvidersEndpoint>> _loggerMock = new();

    [Fact]
    public async Task GetProvidersAsync_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var pagedResult = new PagedResult<ProviderDto>(new List<ProviderDto>(), 0, 1, 10);
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersQuery, Result<PagedResult<ProviderDto>>>(
                It.IsAny<GetProvidersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<ProviderDto>>.Success(pagedResult));

        var methodInfo = typeof(GetProvidersEndpoint).GetMethod("GetProvidersAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        // Parameters: IQueryDispatcher, ILogger<GetProvidersEndpoint>, int pageNumber, int pageSize, string? name, int? type, int? verificationStatus, CancellationToken
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [_queryDispatcherMock.Object, _loggerMock.Object, 1, 10, null, null, null, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<PagedResult<ProviderDto>>>>();
    }

    [Fact]
    public async Task GetProvidersAsync_WhenQueryFails_ShouldReturnBadRequest()
    {
        // Arrange
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersQuery, Result<PagedResult<ProviderDto>>>(
                It.IsAny<GetProvidersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<ProviderDto>>.Failure(Error.BadRequest("Erro de teste")));

        var methodInfo = typeof(GetProvidersEndpoint).GetMethod("GetProvidersAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [_queryDispatcherMock.Object, _loggerMock.Object, 1, 10, null, null, null, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<PagedResult<ProviderDto>>>>();
    }

    [Fact]
    public async Task GetProvidersAsync_OnException_ShouldReturnInternalServerError()
    {
        // Arrange
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersQuery, Result<PagedResult<ProviderDto>>>(
                It.IsAny<GetProvidersQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var methodInfo = typeof(GetProvidersEndpoint).GetMethod("GetProvidersAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [_queryDispatcherMock.Object, _loggerMock.Object, 1, 10, null, null, null, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = (ProblemHttpResult)result;
        problemResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
