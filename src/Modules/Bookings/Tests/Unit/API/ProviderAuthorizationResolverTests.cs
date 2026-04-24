using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Bookings.API.Endpoints.Public;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.API;

public class ProviderAuthorizationResolverTests
{
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<ProviderAuthorizationResolver>> _loggerMock;
    private readonly Mock<IProvidersModuleApi> _providersApiMock;
    private readonly ProviderAuthorizationResolver _sut;

    public ProviderAuthorizationResolverTests()
    {
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<ProviderAuthorizationResolver>>();
        _providersApiMock = new Mock<IProvidersModuleApi>();
        _sut = new ProviderAuthorizationResolver(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnAdmin_When_UserIsSystemAdmin()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.IsSystemAdmin, "true") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _sut.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnAuthorized_When_ProviderIdClaimExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.ProviderId, providerId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _sut.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnUnauthorized_When_NoSubjectClaim()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _sut.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.FailureKind.Should().Be(AuthorizationFailureKind.Unauthorized);
        result.ErrorMessage.Should().Contain("não encontrada");
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnAuthorized_When_ProviderFoundInApi()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var providerDto = new ModuleProviderDto(
            Id: providerId, 
            Name: "Test Provider", 
            Slug: "test-provider", 
            Email: "test@test.com", 
            Document: "12345678901", 
            ProviderType: "Individual", 
            VerificationStatus: "Verified", 
            CreatedAt: DateTime.UtcNow, 
            UpdatedAt: DateTime.UtcNow, 
            IsActive: true);
        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(providerDto));

        // Act
        // Note: MemoryCache extension GetOrCreateAsync is hard to mock, but the logic inside ResolveAsync uses it.
        // We'll rely on the actual implementation of the resolver which calls the API if not in cache.
        // Since we can't easily mock the extension method without a real cache, we'll just test the flow.
        
        // Let's use a real MemoryCache for this test to avoid mocking extension methods
        var realCache = new MemoryCache(new MemoryCacheOptions());
        var sutWithRealCache = new ProviderAuthorizationResolver(realCache, _loggerMock.Object);
        
        var result = await sutWithRealCache.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnNotLinked_When_ProviderNotFoundInApi()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var realCache = new MemoryCache(new MemoryCacheOptions());
        var sutWithRealCache = new ProviderAuthorizationResolver(realCache, _loggerMock.Object);

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(null));

        // Act
        var result = await sutWithRealCache.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.FailureKind.Should().Be(AuthorizationFailureKind.NotLinked);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnUpstreamFailure_When_ApiReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var realCache = new MemoryCache(new MemoryCacheOptions());
        var sutWithRealCache = new ProviderAuthorizationResolver(realCache, _loggerMock.Object);

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(new Error("Api Error", 502)));

        // Act
        var result = await sutWithRealCache.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.FailureKind.Should().Be(AuthorizationFailureKind.UpstreamFailure);
        result.ErrorMessage.Should().Be("Api Error");
        result.ErrorStatusCode.Should().Be(502);
    }
}
