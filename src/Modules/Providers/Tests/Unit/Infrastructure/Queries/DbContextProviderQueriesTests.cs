using System;
using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
public class DbContextProviderQueriesTests : IDisposable
{
    private readonly ProvidersDbContext _context;
    private readonly DbContextProviderQueries _queries;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

    public DbContextProviderQueriesTests()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ProvidersDbContext(options, null!);
        _context.Database.EnsureCreated();
        _queries = new DbContextProviderQueries(_context);
    }

    [Fact]
    public async Task GetPagedAsync_WithNoFilters_ShouldReturnAllActiveProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var provider2 = new ProviderBuilder().Build();
        var deletedProvider = new ProviderBuilder().WithDeleted().Build();

        _context.Providers.AddRange(provider1, provider2, deletedProvider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetPagedAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
        result.Items.Should().NotContain(p => p.Id == deletedProvider.Id);
    }

    [Fact]
    public async Task GetPagedAsync_WithNameFilter_ShouldReturnMatchingProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithName("John Provider").Build();
        var provider2 = new ProviderBuilder().WithName("Jane Specialist").Build();
        var provider3 = new ProviderBuilder().WithName("Bob Provider").Build();

        _context.Providers.AddRange(provider1, provider2, provider3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetPagedAsync(1, 10, nameFilter: "Provider");

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Name.Contains("Provider"));
    }

    [Fact]
    public async Task GetPagedAsync_WithTypeFilter_ShouldReturnProvidersOfType()
    {
        // Arrange
        var individual = new ProviderBuilder().WithType(EProviderType.Individual).Build();
        var company = new ProviderBuilder().WithType(EProviderType.Company).Build();

        _context.Providers.AddRange(individual, company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetPagedAsync(1, 10, typeFilter: EProviderType.Individual);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Type.Should().Be(EProviderType.Individual);
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var providers = Enumerable.Range(1, 25)
            .Select(_ => new ProviderBuilder().Build())
            .ToList();

        _context.Providers.AddRange(providers);
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _queries.GetPagedAsync(1, 10);
        var page2 = await _queries.GetPagedAsync(2, 10);
        var page3 = await _queries.GetPagedAsync(3, 10);

        // Assert
        page1.Items.Should().HaveCount(10);
        page2.Items.Should().HaveCount(10);
        page3.Items.Should().HaveCount(5);
        page1.TotalItems.Should().Be(25);
    }

    [Fact]
    public async Task GetPagedAsync_WithNameFilterCaseInsensitive_ShouldReturnMatchingProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithName("John Provider").Build();
        var provider2 = new ProviderBuilder().WithName("Jane Specialist").Build();

        _context.Providers.AddRange(provider1, provider2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetPagedAsync(1, 10, nameFilter: "provider"); // lowercase

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain(p => p.Name.Contains("Provider", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetPagedAsync_WithStatusFilter_ShouldReturnProvidersWithStatus()
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
        var result = await _queries.GetPagedAsync(
            1, 10, verificationStatusFilter: EVerificationStatus.Verified);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().VerificationStatus.Should().Be(EVerificationStatus.Verified);
    }

    [Fact]
    public async Task GetPagedAsync_WithCombinedFilters_ShouldApplyAllFilters()
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
        var result = await _queries.GetPagedAsync(
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

    [Fact]
    public async Task GetByIdAsync_WithExistingProvider_ShouldReturnProvider()
    {
        // Arrange
        var provider = new ProviderBuilder().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByIdAsync(provider.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedProvider_ShouldReturnNull()
    {
        // Arrange
        var provider = new ProviderBuilder().WithDeleted().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByIdAsync(provider.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WithExistingSlug_ShouldReturnProvider()
    {
        // Arrange
        var provider = new ProviderBuilder().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetBySlugAsync(provider.Slug);

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be(provider.Slug);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUserId_ShouldReturnProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithUserId(userId).Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithExistingUserId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithUserId(userId).Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsByUserIdAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithNonExistingUserId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _queries.ExistsByUserIdAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingProvider_ShouldReturnTrue()
    {
        // Arrange
        var provider = new ProviderBuilder().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsAsync(provider.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithDeletedProvider_ShouldReturnFalse()
    {
        // Arrange
        var provider = new ProviderBuilder().WithDeleted().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsAsync(provider.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdsAsync_WithValidIds_ShouldReturnProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var provider2 = new ProviderBuilder().Build();
        var provider3 = new ProviderBuilder().Build();

        _context.Providers.AddRange(provider1, provider2, provider3);
        await _context.SaveChangesAsync();

        var ids = new List<Guid> { provider1.Id.Value, provider2.Id.Value };

        // Act
        var result = await _queries.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == provider1.Id);
        result.Should().Contain(p => p.Id == provider2.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        var ids = new List<Guid>();

        // Act
        var result = await _queries.GetByIdsAsync(ids);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByVerificationStatusAsync_ShouldReturnMatchingProviders()
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
        var result = await _queries.GetByVerificationStatusAsync(EVerificationStatus.Verified);

        // Assert
        result.Should().HaveCount(1);
        result.First().VerificationStatus.Should().Be(EVerificationStatus.Verified);
    }

    [Fact]
    public async Task GetByTypeAsync_ShouldReturnMatchingProviders()
    {
        // Arrange
        var individual = new ProviderBuilder().WithType(EProviderType.Individual).Build();
        var company = new ProviderBuilder().WithType(EProviderType.Company).Build();

        _context.Providers.AddRange(individual, company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByTypeAsync(EProviderType.Individual);

        // Assert
        result.Should().HaveCount(1);
        result.First().Type.Should().Be(EProviderType.Individual);
    }

    [Fact]
    public async Task GetProviderStatusAsync_WithExistingProvider_ShouldReturnStatus()
    {
        // Arrange
        var provider = new ProviderBuilder()
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var (exists, status) = await _queries.GetProviderStatusAsync(provider.Id);

        // Assert
        exists.Should().BeTrue();
        status.Should().Be(EVerificationStatus.Verified);
    }

    [Fact]
    public async Task GetProviderStatusAsync_WithNonExistingProvider_ShouldReturnFalse()
    {
        // Arrange
        var providerId = new ProviderBuilder().Build().Id;

        // Act
        var (exists, status) = await _queries.GetProviderStatusAsync(providerId);

        // Assert
        exists.Should().BeFalse();
        status.Should().BeNull();
    }

    [Fact]
    public async Task HasProvidersWithServiceAsync_WithExistingService_ShouldReturnTrue()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = new ProviderBuilder().Build();
        provider.UpdateServices(new[] { (serviceId, "Test Service") });
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.HasProvidersWithServiceAsync(serviceId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasProvidersWithServiceAsync_WithNonExistingService_ShouldReturnFalse()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var result = await _queries.HasProvidersWithServiceAsync(serviceId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByDocumentAsync_WithExistingDocument_ShouldReturnProvider()
    {
        // Arrange
        var documentNumber = "12345678900";
        var provider = new ProviderBuilder()
            .WithDocument(documentNumber, MeAjudaAi.Modules.Providers.Domain.Enums.EDocumentType.CPF)
            .Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByDocumentAsync(documentNumber);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
    }

    [Fact]
    public async Task GetByDocumentAsync_WithNonExistingDocument_ShouldReturnNull()
    {
        // Arrange
        var documentNumber = "99999999999";

        // Act
        var result = await _queries.GetByDocumentAsync(documentNumber);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_WithNullList_ShouldReturnEmpty()
    {
        // Arrange
        List<Guid>? ids = null;

        // Act
        var result = await _queries.GetByIdsAsync(ids!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySlugAsync_WithNonExistingSlug_ShouldReturnNull()
    {
        // Arrange
        var slug = "non-existing-slug";

        // Act
        var result = await _queries.GetBySlugAsync(slug);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WithDeletedProvider_ShouldReturnNull()
    {
        // Arrange
        var provider = new ProviderBuilder().WithDeleted().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetBySlugAsync(provider.Slug);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_WithNoResults_ShouldReturnEmptyPagedResult()
    {
        // Arrange - empty database

        // Act
        var result = await _queries.GetPagedAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidPage_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var provider2 = new ProviderBuilder().Build();
        _context.Providers.AddRange(provider1, provider2);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _queries.GetPagedAsync(0, 10);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidPageSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var provider2 = new ProviderBuilder().Build();
        _context.Providers.AddRange(provider1, provider2);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _queries.GetPagedAsync(1, 0);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetPagedAsync_WithNameFilterContainingWildcards_ShouldTreatAsLiteral()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithName("A_B Provider").Build();
        var provider2 = new ProviderBuilder().WithName("A%B Provider").Build();
        var provider3 = new ProviderBuilder().WithName("AB Provider").Build();
        _context.Providers.AddRange(provider1, provider2, provider3);
        await _context.SaveChangesAsync();

        // Act - filter for "A_" should match only literal "A_"
        var result = await _queries.GetPagedAsync(1, 10, nameFilter: "A_");

        // Assert - With SQLite/Relational, "A_" would match "AB" if not escaped
        // This verifies that it matches only the literal "A_B Provider"
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("A_B Provider");
    }

    [Fact]
    public async Task GetByCityAsync_WithMatchingCity_ShouldReturnProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithCity("São Paulo").Build();
        var provider2 = new ProviderBuilder().WithCity("São Paulo").Build();
        var provider3 = new ProviderBuilder().WithCity("Rio de Janeiro").Build();
        _context.Providers.AddRange(provider1, provider2, provider3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByCityAsync("São Paulo");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.BusinessProfile.PrimaryAddress.City == "São Paulo");
    }

    [Fact]
    public async Task GetByCityAsync_WithNoMatchingCity_ShouldReturnEmpty()
    {
        // Arrange
        var provider = new ProviderBuilder().WithCity("São Paulo").Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByCityAsync("Curitiba");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStateAsync_WithMatchingState_ShouldReturnProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithState("SP").Build();
        var provider2 = new ProviderBuilder().WithState("SP").Build();
        var provider3 = new ProviderBuilder().WithState("RJ").Build();
        _context.Providers.AddRange(provider1, provider2, provider3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByStateAsync("SP");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.BusinessProfile.PrimaryAddress.State == "SP");
    }

    [Fact]
    public async Task GetByStateAsync_WithNoMatchingState_ShouldReturnEmpty()
    {
        // Arrange
        var provider = new ProviderBuilder().WithState("SP").Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByStateAsync("RJ");

        // Assert
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
