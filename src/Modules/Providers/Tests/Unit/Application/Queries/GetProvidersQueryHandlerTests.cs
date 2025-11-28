using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
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
    public async Task HandleAsync_WithValidQuery_ShouldReturnPagedProviders()
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithName("Provider 1"),
            ProviderBuilder.Create().WithName("Provider 2"),
            ProviderBuilder.Create().WithName("Provider 3")
        };

        var pagedProviders = new PagedResult<Provider>(providers, 1, 10, 3);
        var query = new GetProvidersQuery(Page: 1, PageSize: 10);

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);

        _providerQueryServiceMock.Verify(
            s => s.GetProvidersAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNameFilter_ShouldApplyFilter()
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithName("John's Plumbing").Build()
        };

        var pagedProviders = new PagedResult<Provider>(providers, 1, 1, 10);
        var query = new GetProvidersQuery(Page: 1, PageSize: 10, Name: "John");

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(1, 10, "John", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Contain("John");

        _providerQueryServiceMock.Verify(
            s => s.GetProvidersAsync(1, 10, "John", null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(EProviderType.Individual)]
    [InlineData(EProviderType.Company)]
    public async Task HandleAsync_WithTypeFilter_ShouldApplyFilter(EProviderType providerType)
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithType(providerType).Build()
        };

        var pagedProviders = new PagedResult<Provider>(providers, 1, 1, 10);
        var query = new GetProvidersQuery(Page: 1, PageSize: 10, Type: (int)providerType);

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(1, 10, null, providerType, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Type.Should().Be(providerType);

        _providerQueryServiceMock.Verify(
            s => s.GetProvidersAsync(1, 10, null, providerType, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(EVerificationStatus.Pending)]
    [InlineData(EVerificationStatus.Verified)]
    [InlineData(EVerificationStatus.Rejected)]
    public async Task HandleAsync_WithVerificationStatusFilter_ShouldApplyFilter(EVerificationStatus status)
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithVerificationStatus(status).Build()
        };

        var pagedProviders = new PagedResult<Provider>(providers, 1, 1, 10);
        var query = new GetProvidersQuery(Page: 1, PageSize: 10, VerificationStatus: (int)status);

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(1, 10, null, null, status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().VerificationStatus.Should().Be(status);

        _providerQueryServiceMock.Verify(
            s => s.GetProvidersAsync(1, 10, null, null, status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create()
                .WithName("John's Company")
                .WithType(EProviderType.Company)
                .WithVerificationStatus(EVerificationStatus.Verified)
                .Build()
        };

        var pagedProviders = new PagedResult<Provider>(providers, 1, 1, 10);
        var query = new GetProvidersQuery(
            Page: 1,
            PageSize: 10,
            Name: "John",
            Type: (int)EProviderType.Company,
            VerificationStatus: (int)EVerificationStatus.Verified);

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(1, 10, "John", EProviderType.Company, EVerificationStatus.Verified, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Contain("John");
        result.Value.Items.First().Type.Should().Be(EProviderType.Company);
        result.Value.Items.First().VerificationStatus.Should().Be(EVerificationStatus.Verified);

        _providerQueryServiceMock.Verify(
            s => s.GetProvidersAsync(1, 10, "John", EProviderType.Company, EVerificationStatus.Verified, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithName("Provider 4").Build(),
            ProviderBuilder.Create().WithName("Provider 5").Build()
        };

        var pagedProviders = new PagedResult<Provider>(providers, 2, 5, 10);
        var query = new GetProvidersQuery(Page: 2, PageSize: 5);

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(2, 5, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalCount.Should().Be(10);
        result.Value.TotalPages.Should().Be(2);

        _providerQueryServiceMock.Verify(
            s => s.GetProvidersAsync(2, 5, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoProvidersFound_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var emptyProviders = new List<Provider>();
        var pagedProviders = new PagedResult<Provider>(emptyProviders, 1, 10, 0);
        var query = new GetProvidersQuery(Page: 1, PageSize: 10);

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetProvidersQuery(Page: 1, PageSize: 10);

        _providerQueryServiceMock
            .Setup(s => s.GetProvidersAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Erro interno ao buscar prestadores");
    }
}
