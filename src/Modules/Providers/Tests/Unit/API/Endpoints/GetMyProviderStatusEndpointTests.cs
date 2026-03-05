using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class GetMyProviderStatusEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;

    public GetMyProviderStatusEndpointTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
    }

    private static System.Reflection.MethodInfo GetMyStatusMethod()
    {
        // O método no endpoint é GetMyStatusAsync e é private static
        var method = typeof(GetMyProviderStatusEndpoint).GetMethod(
            "GetMyStatusAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull("GetMyStatusAsync must exist as a private static method on GetMyProviderStatusEndpoint");
        return method!;
    }

    [Fact]
    public async Task GetMyStatusAsync_WithValidUserId_ShouldReturnStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);
        
        var providerDto = new ProviderDto(
            Guid.NewGuid(), userId, "Test", EProviderType.Individual, null!, 
            EProviderStatus.Active, EVerificationStatus.Verified, EProviderTier.Gold,
            new List<DocumentDto>(), new List<QualificationDto>(), new List<ProviderServiceDto>(), DateTime.UtcNow, null, false, null, null, null);

        var dispatchResult = Result<ProviderDto?>.Success(providerDto);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dispatchResult);

        // Act
        var methodInfo = GetMyStatusMethod();
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, _queryDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Response<ProviderStatusDto>>>();
        var okResult = (Ok<Response<ProviderStatusDto>>)result;
        okResult.Value!.Data.Status.Should().Be(EProviderStatus.Active);
        okResult.Value.Data.VerificationStatus.Should().Be(EVerificationStatus.Verified);
        okResult.Value.Data.Tier.Should().Be(EProviderTier.Gold);

        _queryDispatcherMock.Verify(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyStatusAsync_WithNonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);
        var dispatchResult = Result<ProviderDto?>.Success(null);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dispatchResult);

        // Act
        var methodInfo = GetMyStatusMethod();
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, _queryDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        result.Should().BeOfType<NotFound<Response<object>>>();
    }
}
