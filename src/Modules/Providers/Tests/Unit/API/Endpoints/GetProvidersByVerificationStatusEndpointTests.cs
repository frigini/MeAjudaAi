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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class GetProvidersByVerificationStatusEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock = new();
    private readonly Mock<ILogger<GetProvidersByVerificationStatusEndpoint>> _loggerMock = new();

    [Fact]
    public async Task GetProvidersByVerificationStatusAsync_WithValidStatus_ShouldReturnOk()
    {
        // Arrange
        var status = EVerificationStatus.Verified;
        var providers = new List<ProviderDto> { new ProviderDto() };
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>(
                It.IsAny<GetProvidersByVerificationStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        var methodInfo = typeof(GetProvidersByVerificationStatusEndpoint).GetMethod("GetProvidersByVerificationStatusAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [status, _queryDispatcherMock.Object, _loggerMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<IReadOnlyList<ProviderDto>>>>();
    }

    [Fact]
    public async Task GetProvidersByVerificationStatusAsync_WhenQueryFails_ShouldReturnBadRequest()
    {
        // Arrange
        var status = EVerificationStatus.Pending;
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>(
                It.IsAny<GetProvidersByVerificationStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(GetProvidersByVerificationStatusEndpoint).GetMethod("GetProvidersByVerificationStatusAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [status, _queryDispatcherMock.Object, _loggerMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<IReadOnlyList<ProviderDto>>>>();
    }

    [Fact]
    public async Task GetProvidersByVerificationStatusAsync_OnException_ShouldReturnInternalServerError()
    {
        // Arrange
        var status = EVerificationStatus.Verified;
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>(
                It.IsAny<GetProvidersByVerificationStatusQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        var methodInfo = typeof(GetProvidersByVerificationStatusEndpoint).GetMethod("GetProvidersByVerificationStatusAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [status, _queryDispatcherMock.Object, _loggerMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }
}
