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
    private readonly Mock<CancellationTokenSource> _cancellationTokenSourceMock;

    public GetMyProviderProfileEndpointTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
        _cancellationTokenSourceMock = new Mock<CancellationTokenSource>();
    }

    [Fact]
    public async Task GetMyProfileAsync_WithValidUserId_ShouldDispatchQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateHttpContextWithUserId(userId);
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
        // We need to call the private static method via reflection or change it to public/internal for testing.
        // Or better, just copy the logic or use a wrapper?
        // Since it's a static method mapped to a delegate, we can test the delegate if we can access it.
        // But here I'm testing the method logic. It's private static.
        // Refactoring: Make the method internal or public static for testability.
        // For now, I'll use reflection to invoke it.
        
        var methodInfo = typeof(GetMyProviderProfileEndpoint).GetMethod("GetMyProfileAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
        var task = (Task<IResult>)methodInfo!.Invoke(null, new object[] { context, _queryDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        // Assert
        // Result should be OK with ProviderDto
        // Since BaseEndpoint.Handle returns IResult, checking it depends on implementation (TypedResults vs others)
        // BaseEndpoint.Handle uses EndpointExtensions.Handle which returns IResult.
        // It's hard to assert IResult specific content without specific helpers.
        // But verifying the dispatcher call ensures logic flow.
        
        _queryDispatcherMock.Verify(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    private DefaultHttpContext CreateHttpContextWithUserId(Guid userId)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }
}
