using System;
using System.Threading.Tasks;
using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Persistence;

[Trait("Category", "Unit")]
public class ProvidersDbContextRepositoryTests : IDisposable
{
    private readonly ProvidersDbContext _context;

    public ProvidersDbContextRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProvidersDbContext(options, null!);
    }

    [Fact]
    public void GetRepository_ForProvider_ShouldReturnSelf()
    {
        // Arrange & Act
        var repository = (IRepository<Provider, ProviderId>)_context;

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeSameAs(_context);
    }

    [Fact]
    public async Task TryFindAsync_WithExistingProvider_ShouldReturnProvider()
    {
        // Arrange
        var provider = new ProviderBuilder().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _context.TryFindAsync(provider.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
    }

    [Fact]
    public async Task TryFindAsync_WithNonExistingProvider_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = new ProviderId(Guid.NewGuid());

        // Act
        var result = await _context.TryFindAsync(nonExistingId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TryFindAsync_WithDeletedProvider_ShouldReturnNull()
    {
        // Arrange
        var provider = new ProviderBuilder().WithDeleted().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _context.TryFindAsync(provider.Id, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_ShouldPersistProvider()
    {
        // Arrange
        var provider = new ProviderBuilder().Build();

        // Act
        _context.Add(provider);
        await _context.SaveChangesAsync();

        // Assert
        var persisted = await _context.Providers.FirstOrDefaultAsync(p => p.Id == provider.Id);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(provider.Id);
    }

    [Fact]
    public async Task Delete_ShouldRemoveProvider()
    {
        // Arrange
        var provider = new ProviderBuilder().Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        _context.Delete(provider);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Providers.FirstOrDefaultAsync(p => p.Id == provider.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task TryFindAsync_ShouldIncludeDocuments()
    {
        // Arrange
        var provider = new ProviderBuilder()
            .WithDocument("12345678900", MeAjudaAi.Modules.Providers.Domain.Enums.EDocumentType.CPF)
            .Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _context.TryFindAsync(provider.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Documents.Should().HaveCount(1);
    }

    [Fact]
    public async Task TryFindAsync_ShouldIncludeQualifications()
    {
        // Arrange
        var provider = new ProviderBuilder()
            .WithQualification("Certification", "Description", "Organization", DateTime.Now, DateTime.Now.AddYears(1), "DOC123")
            .Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _context.TryFindAsync(provider.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Qualifications.Should().HaveCount(1);
    }

    [Fact]
    public async Task TryFindAsync_ShouldIncludeServices()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = new ProviderBuilder().Build();
        provider.UpdateServices(new[] { (serviceId, "Test Service") });
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _context.TryFindAsync(provider.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Services.Should().HaveCount(1);
        result.Services.First().ServiceId.Should().Be(serviceId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
