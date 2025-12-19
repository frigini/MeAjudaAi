using System;
using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
public class ProviderQueryServiceTests : IDisposable
{
    private readonly ProvidersDbContext _context;
    private readonly ProviderQueryService _service;

    public ProviderQueryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProvidersDbContext(options);
        _service = new ProviderQueryService(_context);
    }

    [Fact]
    public async Task GetProvidersAsync_WithNoFilters_ShouldReturnAllActiveProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var provider2 = new ProviderBuilder().Build();
        var deletedProvider = new ProviderBuilder().WithDeleted().Build();

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

    [Fact]
    public async Task GetProvidersAsync_WithNameFilterCaseInsensitive_ShouldReturnMatchingProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithName("John Provider").Build();
        var provider2 = new ProviderBuilder().WithName("Jane Specialist").Build();

        _context.Providers.AddRange(provider1, provider2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProvidersAsync(1, 10, nameFilter: "provider"); // lowercase

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain(p => p.Name.Contains("Provider", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetProvidersAsync_WithStatusFilter_ShouldReturnProvidersWithStatus()
    {
        // Arrange
        var verified = new ProviderBuilder()
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();
        var pending = new ProviderBuilder()
            .WithVerificationStatus(EVerificationStatus.Pending)
            .Build();

        _context.Providers.AddRange(verified, pending);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProvidersAsync(
            1, 10, verificationStatusFilter: EVerificationStatus.Verified);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().VerificationStatus.Should().Be(EVerificationStatus.Verified);
    }

    [Fact]
    public async Task GetProvidersAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var targetProvider = new ProviderBuilder()
            .WithName("John Provider")
            .WithType(EProviderType.Individual)
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();
        var otherProvider1 = new ProviderBuilder()
            .WithName("Jane Provider")
            .WithType(EProviderType.Company)
            .Build();
        var otherProvider2 = new ProviderBuilder()
            .WithName("Bob Specialist")
            .WithType(EProviderType.Individual)
            .Build();

        _context.Providers.AddRange(targetProvider, otherProvider1, otherProvider2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProvidersAsync(
            1, 10, 
            nameFilter: "Provider",
            typeFilter: EProviderType.Individual,
            verificationStatusFilter: EVerificationStatus.Verified);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("John Provider");
        result.Items.First().Type.Should().Be(EProviderType.Individual);
        result.Items.First().VerificationStatus.Should().Be(EVerificationStatus.Verified);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
