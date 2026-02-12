using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Security.Claims;
using Xunit;
using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class GetMyProviderProfileEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;

    public GetMyProviderProfileEndpointTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
    }

    private static System.Reflection.MethodInfo GetMyProfileMethod()
    {
        var method = typeof(GetMyProviderProfileEndpoint).GetMethod(
            "GetMyProfileAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull("GetMyProfileAsync must exist as a private static method on GetMyProviderProfileEndpoint");
        return method!;
    }

    [Fact]
    public async Task GetMyProfileAsync_WithValidUserId_ShouldDispatchQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);
        var providerDto = new ProviderDto(
            Guid.NewGuid(), userId, "Test", EProviderType.Individual, null!, 
            EProviderStatus.Active, EVerificationStatus.Verified, 
            new List<DocumentDto>(), new List<QualificationDto>(), new List<ProviderServiceDto>(), DateTime.UtcNow, null, false, null, null, null);

        var dispatchResult = Result<ProviderDto?>.Success(providerDto);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dispatchResult);

        // Act
        // Act
        var methodInfo = GetMyProfileMethod();
            
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, _queryDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<ProviderDto?>>>();
        _queryDispatcherMock.Verify(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyProfileAsync_WithInvalidUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", "invalid-guid")
        }));

        // Act
        // Act
        var methodInfo = GetMyProfileMethod();
            
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, _queryDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Response<object>>>();
        _queryDispatcherMock.Verify(
            x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyProfileAsync_WithNonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);
        var dispatchResult = Result<ProviderDto?>.Success(null);

        _queryDispatcherMock
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dispatchResult);

        // Act
        var methodInfo = GetMyProfileMethod();
            
        var task = (Task<IResult>)methodInfo.Invoke(null, new object[] { context, _queryDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        result.Should().BeOfType<NotFound<Response<object>>>();
    }
}
