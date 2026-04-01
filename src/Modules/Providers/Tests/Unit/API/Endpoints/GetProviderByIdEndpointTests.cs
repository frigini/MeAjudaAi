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
public class GetProviderByIdEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock = new();

    [Fact]
    public async Task GetProviderAsync_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerDto = new ProviderDto { Id = providerId };
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByIdQuery, Result<ProviderDto?>>(
                It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        var methodInfo = typeof(GetProviderByIdEndpoint).GetMethod("GetProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, _queryDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<ProviderDto?>>>();
    }

    [Fact]
    public async Task GetProviderAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByIdQuery, Result<ProviderDto?>>(
                It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Failure(Error.NotFound("Not Found")));

        var methodInfo = typeof(GetProviderByIdEndpoint).GetMethod("GetProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, _queryDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<NotFound<Result<ProviderDto?>>>();
    }
}
