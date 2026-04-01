using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class GetPublicProviderByIdOrSlugEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock = new();

    [Fact]
    public async Task GetPublicProviderAsync_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var idOrSlug = Guid.NewGuid().ToString();
        var providerDto = new PublicProviderDto { Name = "Public Provider" };
        var context = new DefaultHttpContext();

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetPublicProviderByIdOrSlugQuery, Result<PublicProviderDto?>>(
                It.IsAny<GetPublicProviderByIdOrSlugQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PublicProviderDto?>.Success(providerDto));

        var methodInfo = typeof(GetPublicProviderByIdOrSlugEndpoint).GetMethod("GetPublicProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [idOrSlug, _queryDispatcherMock.Object, context, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<PublicProviderDto?>>>();
    }

    [Fact]
    public async Task GetPublicProviderAsync_WhenAuthenticated_ShouldPassAuthenticatedToQuery()
    {
        // Arrange
        var idOrSlug = "slug-teste";
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity("TestAuth");
        context.User = new ClaimsPrincipal(identity);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetPublicProviderByIdOrSlugQuery, Result<PublicProviderDto?>>(
                It.Is<GetPublicProviderByIdOrSlugQuery>(q => q.IdOrSlug == idOrSlug && q.IsAuthenticated), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PublicProviderDto?>.Success(new PublicProviderDto()));

        var methodInfo = typeof(GetPublicProviderByIdOrSlugEndpoint).GetMethod("GetPublicProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [idOrSlug, _queryDispatcherMock.Object, context, CancellationToken.None])!;
        await task;

        // Assert
        _queryDispatcherMock.Verify();
    }

    [Fact]
    public async Task GetPublicProviderAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var idOrSlug = "non-existent";
        var context = new DefaultHttpContext();

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetPublicProviderByIdOrSlugQuery, Result<PublicProviderDto?>>(
                It.IsAny<GetPublicProviderByIdOrSlugQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PublicProviderDto?>.Failure(Error.NotFound("Not Found")));

        var methodInfo = typeof(GetPublicProviderByIdOrSlugEndpoint).GetMethod("GetPublicProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [idOrSlug, _queryDispatcherMock.Object, context, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<NotFound<Result<PublicProviderDto?>>>();
    }
}
