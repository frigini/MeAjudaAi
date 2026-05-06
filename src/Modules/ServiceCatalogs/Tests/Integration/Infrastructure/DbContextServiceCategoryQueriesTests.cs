using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration.Infrastructure;

public sealed class DbContextServiceCategoryQueriesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("catalogs_test")
        .Build();
    
    private ServiceCatalogsDbContext? _dbContext;
    private DbContextServiceCategoryQueries? _queries;

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        _dbContext = new ServiceCatalogsDbContext(options, new Mock<MeAjudaAi.Shared.Events.IDomainEventProcessor>().Object);
        await _dbContext.Database.MigrateAsync();
        _queries = new DbContextServiceCategoryQueries(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null) await _dbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOrderedCategories()
    {
        _dbContext!.ServiceCategories.Add(new ServiceCategoryBuilder().WithName("B").WithDisplayOrder(2).Build());
        _dbContext.ServiceCategories.Add(new ServiceCategoryBuilder().WithName("A").WithDisplayOrder(1).Build());
        await _dbContext.SaveChangesAsync();

        var categories = await _queries!.GetAllAsync(activeOnly: false);
        
        categories.Should().HaveCount(2);
        categories[0].Name.Should().Be("A");
        categories[1].Name.Should().Be("B");
    }
}
