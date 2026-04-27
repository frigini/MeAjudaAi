using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
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
    private readonly Mock<ICacheService> _cacheMock;
    private readonly ProviderAuthorizationResolver _sut;

    public ProviderAuthorizationResolverTests()
    {
        _loggerMock = new Mock<ILogger<ProviderAuthorizationResolver>>();
        _providersApiMock = new Mock<IProvidersModuleApi>();
        _cacheMock = new Mock<ICacheService>();
        _sut = new ProviderAuthorizationResolver(_cacheMock.Object, _providersApiMock.Object, _loggerMock.Object);

        // Setup padrão: executa o factory
        _cacheMock.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<ProviderResolutionResult>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<CancellationToken, ValueTask<ProviderResolutionResult>>, TimeSpan?, HybridCacheEntryOptions?, IReadOnlyCollection<string>?, CancellationToken>(
                async (key, factory, exp, opt, tags, ct) => await factory(ct));
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnAdmin_When_UserIsSystemAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] 
        { 
            new Claim(AuthConstants.Claims.Subject, userId.ToString()),
            new Claim(AuthConstants.Claims.IsSystemAdmin, "true") 
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _sut.ResolveAsync(principal);

        // Assert
        result.IsAdmin.Should().BeTrue();
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnAuthorized_When_ProviderIdClaimExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var claims = new[] 
        { 
            new Claim(AuthConstants.Claims.Subject, userId.ToString()),
            new Claim(AuthConstants.Claims.ProviderId, providerId.ToString()) 
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _sut.ResolveAsync(principal);

        // Assert
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnAuthorized_UsingNameIdentifier_WhenSubjectMissing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var providerId = Guid.NewGuid();
        var providerDto = CreateModuleProviderDto(providerId);
        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.ResolveAsync(principal);

        // Assert
        result.ProviderId.Should().Be(providerId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnUnauthorized_When_NoUserIdentification()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _sut.ResolveAsync(principal);

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
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var providerDto = CreateModuleProviderDto(providerId);
        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.ResolveAsync(principal);

        // Assert
        result.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task AuthorizeBookingOperationAsync_Should_ReturnSuccess_WhenUserIsAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] 
        { 
            new Claim(AuthConstants.Claims.Subject, userId.ToString()),
            new Claim(AuthConstants.Claims.IsSystemAdmin, "true") 
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _sut.AuthorizeBookingOperationAsync(principal, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeBookingOperationAsync_Should_ReturnSuccess_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(null));

        // Act
        var result = await _sut.AuthorizeBookingOperationAsync(principal, userId, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeBookingOperationAsync_Should_ReturnSuccess_WhenUserIsProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var providerDto = CreateModuleProviderDto(providerId);
        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.AuthorizeBookingOperationAsync(principal, Guid.NewGuid(), providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeBookingOperationAsync_Should_ReturnForbidden_WhenUserIsNotAdminOwnerOrProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(AuthConstants.Claims.Subject, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _providersApiMock.Setup(x => x.GetProviderByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderDto?>.Success(null));

        // Act
        var result = await _sut.AuthorizeBookingOperationAsync(principal, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvalidateAsync_Should_RemoveFromCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedKey = $"bookings:provider_by_user:{userId}";

        // Act
        await _sut.InvalidateAsync(userId);

        // Assert
        _cacheMock.Verify(x => x.RemoveAsync(expectedKey, It.IsAny<CancellationToken>()), Times.Once);
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
