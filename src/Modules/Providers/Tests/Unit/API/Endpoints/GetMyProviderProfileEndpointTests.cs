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
        var methodInfo = typeof(GetMyProviderProfileEndpoint).GetMethod("GetMyProfileAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
        var task = (Task<IResult>)methodInfo!.Invoke(null, new object[] { context, _queryDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        _queryDispatcherMock.Verify(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

}
