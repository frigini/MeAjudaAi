using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.ModuleApi;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders.Enums;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;
using DomainEnums = MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Application")]
public class SearchProvidersModuleApiTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;
    private readonly Mock<ISearchableProviderRepository> _repositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersApiMock;
    private readonly Mock<ILogger<SearchProvidersModuleApi>> _loggerMock;
    private readonly SearchProvidersModuleApi _sut;

    public SearchProvidersModuleApiTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
        _repositoryMock = new Mock<ISearchableProviderRepository>();
        _providersApiMock = new Mock<IProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<SearchProvidersModuleApi>>();

        _sut = new SearchProvidersModuleApi(
            _queryDispatcherMock.Object,
            _repositoryMock.Object,
            _providersApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturn_SearchProviders()
    {
        // Act
        var result = _sut.ModuleName;

        // Assert
        result.Should().Be("SearchProviders");
    }

    [Fact]
    public void ApiVersion_ShouldReturn_Version1()
    {
        // Act
        var result = _sut.ApiVersion;

        // Assert
        result.Should().Be("1.0");
    }

    #region IsAvailableAsync Tests

    [Fact]
    public async Task IsAvailableAsync_WhenSearchSucceeds_ShouldReturnTrue()
    {
        // Arrange
        var pagedResult = new PagedResult<SearchableProviderDto>(
            new List<SearchableProviderDto>(),
            0,
            1,
            1);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SearchableProviderDto>>.Success(pagedResult));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenSearchFails_ShouldReturnFalse()
    {
        // Arrange
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SearchableProviderDto>>.Failure("Query failed"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenQueryThrows_ShouldReturnFalse()
    {
        // Arrange
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.IsAvailableAsync(cts.Token));
    }

    #endregion

    #region SearchProvidersAsync Tests

    [Fact]
    public async Task SearchProvidersAsync_WithValidParameters_ShouldReturnResults()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var providers = new List<SearchableProviderDto>
        {
            new()
            {
                ProviderId = providerId,
                Name = "Provider 1",
                Description = "Test provider",
                Location = new LocationDto { Latitude = -23.5, Longitude = -46.6 },
                AverageRating = 4.5m,
                TotalReviews = 10,
                SubscriptionTier = DomainEnums.ESubscriptionTier.Gold,
                ServiceIds = new[] { serviceId },
                DistanceInKm = 1.5,
                City = "São Paulo",
                State = "SP"
            }
        };

        var pagedResult = new PagedResult<SearchableProviderDto>(providers, 1, 20, providers.Count);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SearchableProviderDto>>.Success(pagedResult));

        // Act
        var result = await _sut.SearchProvidersAsync(
            latitude: -23.561414,
            longitude: -46.656559,
            radiusInKm: 5.0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verifica propriedades de ModulePagedSearchResultDto
        result.Value!.TotalCount.Should().Be(1);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.Items.Should().HaveCount(1);

        // Verifica propriedades de ModuleSearchableProviderDto
        var provider = result.Value.Items[0];
        provider.ProviderId.Should().Be(providerId);
        provider.Name.Should().Be("Provider 1");
        provider.Description.Should().Be("Test provider");
        provider.AverageRating.Should().Be(4.5m);
        provider.TotalReviews.Should().Be(10);
        provider.SubscriptionTier.Should().Be(ESubscriptionTier.Gold);
        provider.ServiceIds.Should().ContainSingle().And.Contain(serviceId);
        provider.DistanceInKm.Should().Be(1.5);
        provider.City.Should().Be("São Paulo");
        provider.State.Should().Be("SP");

        // Verifica propriedades de ModuleLocationDto
        provider.Location.Should().NotBeNull();
        provider.Location!.Latitude.Should().Be(-23.5);
        provider.Location.Longitude.Should().Be(-46.6);
    }

    [Fact]
    public async Task SearchProvidersAsync_WithFilters_ShouldPassParametersToQuery()
    {
        // Arrange
        var serviceIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var tiers = new[] { ESubscriptionTier.Gold, ESubscriptionTier.Platinum };
        var pagedResult = new PagedResult<SearchableProviderDto>(new List<SearchableProviderDto>(), 0, 1, 10);

        SearchProvidersQuery? capturedQuery = null;
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<SearchProvidersQuery, CancellationToken>((query, _) => capturedQuery = query)
            .ReturnsAsync(Result<PagedResult<SearchableProviderDto>>.Success(pagedResult));

        // Act
        var result = await _sut.SearchProvidersAsync(
            latitude: -23.561414,
            longitude: -46.656559,
            radiusInKm: 10.0,
            serviceIds: serviceIds,
            minRating: 4.0m,
            subscriptionTiers: tiers,
            pageNumber: 2,
            pageSize: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedQuery.Should().NotBeNull();
        capturedQuery!.Latitude.Should().Be(-23.561414);
        capturedQuery.Longitude.Should().Be(-46.656559);
        capturedQuery.RadiusInKm.Should().Be(10.0);
        capturedQuery.ServiceIds.Should().BeEquivalentTo(serviceIds);
        capturedQuery.MinRating.Should().Be(4.0m);
        capturedQuery.SubscriptionTiers.Should().BeEquivalentTo(new[]
        {
            DomainEnums.ESubscriptionTier.Gold,
            DomainEnums.ESubscriptionTier.Platinum
        });
        capturedQuery.Page.Should().Be(2);
        capturedQuery.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task SearchProvidersAsync_WhenQueryFails_ShouldReturnFailure()
    {
        // Arrange
        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SearchableProviderDto>>.Failure("Query execution failed"));

        // Act
        var result = await _sut.SearchProvidersAsync(-23.561414, -46.656559, 5.0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Query execution failed");
    }

    [Fact]
    public async Task SearchProvidersAsync_WithNullOptionalParameters_ShouldSucceed()
    {
        // Arrange
        var pagedResult = new PagedResult<SearchableProviderDto>(new List<SearchableProviderDto>(), 0, 1, 20);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(
                It.IsAny<SearchProvidersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SearchableProviderDto>>.Success(pagedResult));

        // Act
        var result = await _sut.SearchProvidersAsync(-23.561414, -46.656559, 5.0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    #endregion

    #region IndexProviderAsync Tests

    [Fact]
    public async Task IndexProviderAsync_WithNewProvider_ShouldCreateAndIndex()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerData = new ModuleProviderIndexingDto
        {
            ProviderId = providerId,
            Name = "New Provider",
            Description = "Test description",
            Latitude = -23.561414,
            Longitude = -46.656559,
            City = "São Paulo",
            State = "SP",
            AverageRating = 4.5m,
            TotalReviews = 10,
            SubscriptionTier = ESubscriptionTier.Gold,
            ServiceIds = new[] { Guid.NewGuid() },
            IsActive = true
        };

        _providersApiMock
            .Setup(x => x.GetProviderForIndexingAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderIndexingDto?>.Success(providerData));

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SearchableProvider?)null);

        // Act
        var result = await _sut.IndexProviderAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<SearchableProvider>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IndexProviderAsync_WithExistingProvider_ShouldUpdate()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var existingProvider = SearchableProvider.Create(
            providerId,
            "Old Name",
            new GeoPoint(-23.5, -46.6),
            DomainEnums.ESubscriptionTier.Free,
            "Old description",
            "Old City",
            "OC");

        var updatedData = new ModuleProviderIndexingDto
        {
            ProviderId = providerId,
            Name = "Updated Provider",
            Description = "Updated description",
            Latitude = -23.561414,
            Longitude = -46.656559,
            City = "São Paulo",
            State = "SP",
            AverageRating = 4.8m,
            TotalReviews = 25,
            SubscriptionTier = ESubscriptionTier.Platinum,
            ServiceIds = new[] { Guid.NewGuid(), Guid.NewGuid() },
            IsActive = true
        };

        _providersApiMock
            .Setup(x => x.GetProviderForIndexingAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderIndexingDto?>.Success(updatedData));

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _sut.IndexProviderAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.UpdateAsync(existingProvider, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IndexProviderAsync_WhenProviderNotFoundInProvidersModule_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _providersApiMock
            .Setup(x => x.GetProviderForIndexingAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderIndexingDto?>.Success(null));

        // Act
        var result = await _sut.IndexProviderAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not found");
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<SearchableProvider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IndexProviderAsync_WhenProvidersApiFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _providersApiMock
            .Setup(x => x.GetProviderForIndexingAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderIndexingDto?>.Failure("Providers module unavailable"));

        // Act
        var result = await _sut.IndexProviderAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Providers module unavailable");
    }

    [Fact]
    public async Task IndexProviderAsync_WhenInactiveProvider_ShouldDeactivateInIndex()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerData = new ModuleProviderIndexingDto
        {
            ProviderId = providerId,
            Name = "Inactive Provider",
            Description = "Test",
            Latitude = -23.5,
            Longitude = -46.6,
            City = "São Paulo",
            State = "SP",
            AverageRating = 0,
            TotalReviews = 0,
            SubscriptionTier = ESubscriptionTier.Free,
            ServiceIds = Array.Empty<Guid>(),
            IsActive = false
        };

        _providersApiMock
            .Setup(x => x.GetProviderForIndexingAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleProviderIndexingDto?>.Success(providerData));

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SearchableProvider?)null);

        // Act
        var result = await _sut.IndexProviderAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.AddAsync(
                It.Is<SearchableProvider>(p => !p.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IndexProviderAsync_WhenGetProviderThrows_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _providersApiMock
            .Setup(x => x.GetProviderForIndexingAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.IndexProviderAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("internal error");
    }

    #endregion

    #region RemoveProviderAsync Tests

    [Fact]
    public async Task RemoveProviderAsync_WithExistingProvider_ShouldRemoveFromIndex()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var existingProvider = SearchableProvider.Create(
            providerId,
            "Provider to Remove",
            new GeoPoint(-23.5, -46.6),
            DomainEnums.ESubscriptionTier.Free,
            "Test",
            "São Paulo",
            "SP");

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _sut.RemoveProviderAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(existingProvider, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveProviderAsync_WithNonExistentProvider_ShouldSucceedIdempotently()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SearchableProvider?)null);

        // Act
        var result = await _sut.RemoveProviderAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<SearchableProvider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveProviderAsync_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.RemoveProviderAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("internal error");
    }

    #endregion
}
