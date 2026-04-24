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

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Component", "Authorization")]
public class ProviderAuthorizationResolverTests
{
    private readonly Mock<ILogger<ProviderAuthorizationResolver>> _loggerMock;
    private readonly Mock<IProvidersModuleApi> _providersApiMock;
    private readonly ProviderAuthorizationResolver _sut;

    public ProviderAuthorizationResolverTests()
    {
        // Usamos um MemoryCache real para exercitar o caminho de cache (extensões de cache são difíceis de mockar)
        var realCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<ProviderAuthorizationResolver>>();
        _providersApiMock = new Mock<IProvidersModuleApi>();
        _sut = new ProviderAuthorizationResolver(realCache, _loggerMock.Object);
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
    public async Task ResolveAsync_Should_ReturnUnauthorized_When_SubjectClaimIsInvalid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, "not-a-guid") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _sut.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.FailureKind.Should().Be(AuthorizationFailureKind.Unauthorized);
        result.ErrorMessage.Should().Contain("inválido");
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

        var providerDto = CreateModuleProviderDto(providerId);
        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task ResolveAsync_Should_HitCache_On_SecondCall()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var providerDto = CreateModuleProviderDto(providerId);
        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(providerDto));

        // Act
        var firstResult = await _sut.ResolveAsync(context, _providersApiMock.Object);
        var secondResult = await _sut.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        firstResult.ProviderId.Should().Be(providerId);
        secondResult.ProviderId.Should().Be(providerId);
        
        // Verifica que a API foi chamada apenas uma vez apesar de duas resoluções
        _providersApiMock.Verify(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnNotLinked_When_ProviderNotFoundInApi()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(null));

        // Act
        var result = await _sut.ResolveAsync(context, _providersApiMock.Object);

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

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Failure(new Error("Api Error", 502)));

        // Act
        var result = await _sut.ResolveAsync(context, _providersApiMock.Object);

        // Assert
        result.FailureKind.Should().Be(AuthorizationFailureKind.UpstreamFailure);
        result.ErrorMessage.Should().Be("Api Error");
        result.ErrorStatusCode.Should().Be(502);
    }

    private static ModuleProviderDto CreateModuleProviderDto(Guid providerId)
    {
        return new ModuleProviderDto(
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
    }
}
