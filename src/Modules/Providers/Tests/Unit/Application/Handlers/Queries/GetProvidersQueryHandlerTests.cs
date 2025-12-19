using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProvidersQueryHandlerTests
{
    private readonly Mock<IProviderQueryService> _providerQueryServiceMock;
    private readonly Mock<ILogger<GetProvidersQueryHandler>> _loggerMock;
    private readonly GetProvidersQueryHandler _handler;

    public GetProvidersQueryHandlerTests()
    {
        _providerQueryServiceMock = new Mock<IProviderQueryService>();
        _loggerMock = new Mock<ILogger<GetProvidersQueryHandler>>();
        _handler = new GetProvidersQueryHandler(_providerQueryServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var providers = new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>
        {
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build()
        };

        var pagedProviders = new PagedResult<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>(providers, 1, 10, 2);

        _providerQueryServiceMock
            .Setup(x => x.GetProvidersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<EProviderType?>(),
                It.IsAny<EVerificationStatus?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        var query = new GetProvidersQuery(1, 10);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(2);

        _providerQueryServiceMock.Verify(
            x => x.GetProvidersAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithFilters_ShouldApplyFiltersCorrectly()
    {
        // Arrange
        var nameFilter = "John";
        var typeFilter = EProviderType.Individual;
        var statusFilter = EVerificationStatus.Verified;

        var providers = new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>
        {
            new ProviderBuilder()
                .WithName(nameFilter)
                .WithType(typeFilter)
                .WithVerificationStatus(statusFilter)
                .Build()
        };

        var pagedProviders = new PagedResult<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>(providers, 1, 10, 1);

        _providerQueryServiceMock
            .Setup(x => x.GetProvidersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                nameFilter,
                typeFilter,
                statusFilter,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        var query = new GetProvidersQuery(1, 10, nameFilter, (int)typeFilter, (int)statusFilter);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);

        _providerQueryServiceMock.Verify(
            x => x.GetProvidersAsync(1, 10, nameFilter, typeFilter, statusFilter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyResult_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var pagedProviders = new PagedResult<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>(
            new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>(), 1, 10, 0);

        _providerQueryServiceMock
            .Setup(x => x.GetProvidersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<EProviderType?>(),
                It.IsAny<EVerificationStatus?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        var query = new GetProvidersQuery(1, 10);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceThrowsException_ShouldReturnFailure()
    {
        // Arrange
        _providerQueryServiceMock
            .Setup(x => x.GetProvidersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<EProviderType?>(),
                It.IsAny<EVerificationStatus?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetProvidersQuery(1, 10);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}
