using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Persistence;

public class SearchProvidersDbContextModelTests
{
    [Fact]
    public void OnModelCreating_ShouldConfigureModelCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SearchProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: "SearchProvidersTestDb_" + Guid.NewGuid())
            .Options;

        // Act
        using var context = new SearchProvidersDbContext(options);
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetDefaultSchema().Should().Be("search_providers");

        var providerType = model.FindEntityType(typeof(SearchableProvider));
        providerType.Should().NotBeNull();
        providerType!.GetSchema().Should().Be("search_providers");
    }

    [Fact]
    public void GetRepository_WithSupportedType_ShouldReturnRepository()
    {
        var options = new DbContextOptionsBuilder<SearchProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: "SearchProvidersTestDb_" + Guid.NewGuid())
            .Options;

        using var context = new SearchProvidersDbContext(options);

        var repo = context.GetRepository<SearchableProvider, SearchableProviderId>();

        repo.Should().NotBeNull();
        repo.Should().BeAssignableTo<IRepository<SearchableProvider, SearchableProviderId>>();
    }

    [Fact]
    public void GetRepository_WithUnsupportedType_ShouldThrowInvalidOperationException()
    {
        var options = new DbContextOptionsBuilder<SearchProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: "SearchProvidersTestDb_" + Guid.NewGuid())
            .Options;

        using var context = new SearchProvidersDbContext(options);

        var act = () => context.GetRepository<object, Guid>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SearchProvidersDbContext does not implement*");
    }
}


