using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class GetProvidersByTypeEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock = new();

    [Fact]
    public async Task GetProvidersByTypeAsync_WithValidType_ShouldReturnOk()
    {
        // Arrange
        var type = EProviderType.Individual;
        var providers = new List<ProviderDto> { new ProviderDto() };
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>(
                It.IsAny<GetProvidersByTypeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        var methodInfo = typeof(GetProvidersByTypeEndpoint).GetMethod("GetProvidersByTypeAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [type, _queryDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<IReadOnlyList<ProviderDto>>>>();
    }

    [Fact]
    public async Task GetProvidersByTypeAsync_WhenQueryFails_ShouldReturnBadRequest()
    {
        // Arrange
        var type = EProviderType.Company;
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>(
                It.IsAny<GetProvidersByTypeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(GetProvidersByTypeEndpoint).GetMethod("GetProvidersByTypeAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [type, _queryDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<IReadOnlyList<ProviderDto>>>>();
    }
}
