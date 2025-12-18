using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
public class ProviderQueryServiceTests
{
    private readonly Mock<ILogger<ProviderQueryService>> _loggerMock;
    private readonly ProvidersDbContext _context;
    private readonly ProviderQueryService _service;

    public ProviderQueryServiceTests()
    {
        _loggerMock = new Mock<ILogger<ProviderQueryService>>();

        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProvidersDbContext(options);
        _service = new ProviderQueryService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProvidersAsync_WithNoFilters_ShouldReturnAllActiveProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var provider2 = new ProviderBuilder().Build();
        var deletedProvider = new ProviderBuilder().Build();
        
        // Simular soft delete
        deletedProvider.GetType().GetProperty("IsDeleted")?.SetValue(deletedProvider, true);

        _context.Providers.AddRange(provider1, provider2, deletedProvider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProvidersAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(p => p.Id == deletedProvider.Id);
    }

    [Fact]
    public async Task GetProvidersAsync_WithNameFilter_ShouldReturnMatchingProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithName("John Provider").Build();
        var provider2 = new ProviderBuilder().WithName("Jane Specialist").Build();
        var provider3 = new ProviderBuilder().WithName("Bob Provider").Build();

        _context.Providers.AddRange(provider1, provider2, provider3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProvidersAsync(1, 10, nameFilter: "Provider");

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Name.Contains("Provider"));
    }

    [Fact]
    public async Task GetProvidersAsync_WithTypeFilter_ShouldReturnProvidersOfType()
    {
        // Arrange
        var individual = new ProviderBuilder().WithType(EProviderType.Individual).Build();
        var company = new ProviderBuilder().WithType(EProviderType.Company).Build();

        _context.Providers.AddRange(individual, company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProvidersAsync(1, 10, typeFilter: EProviderType.Individual);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Type.Should().Be(EProviderType.Individual);
    }

    [Fact]
    public async Task GetProvidersAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var providers = Enumerable.Range(1, 25)
            .Select(_ => new ProviderBuilder().Build())
            .ToList();

        _context.Providers.AddRange(providers);
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _service.GetProvidersAsync(1, 10);
        var page2 = await _service.GetProvidersAsync(2, 10);
        var page3 = await _service.GetProvidersAsync(3, 10);

        // Assert
        page1.Items.Should().HaveCount(10);
        page2.Items.Should().HaveCount(10);
        page3.Items.Should().HaveCount(5);
        page1.TotalCount.Should().Be(25);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
