using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration.Infrastructure;

public sealed class DbContextServiceQueriesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("catalogs_test")
        .Build();
    
    private ServiceCatalogsDbContext? _dbContext;
    private DbContextServiceQueries? _queries;

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        _dbContext = new ServiceCatalogsDbContext(options, new Mock<MeAjudaAi.Shared.Events.IDomainEventProcessor>().Object);
        await _dbContext.Database.MigrateAsync();
        _queries = new DbContextServiceQueries(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null) await _dbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByActiveOnlyAndName()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        _dbContext!.ServiceCategories.Add(category);
        
        _dbContext.Services.Add(new ServiceBuilder().WithName("Serviço A").WithCategoryId(category.Id).AsActive().Build());
        _dbContext.Services.Add(new ServiceBuilder().WithName("Serviço B").WithCategoryId(category.Id).AsInactive().Build());
        await _dbContext.SaveChangesAsync();

        var active = await _queries!.GetAllAsync(activeOnly: true);
        active.Should().HaveCount(1);
        active[0].Name.Should().Be("Serviço A");

        var all = await _queries.GetAllAsync(activeOnly: false, name: "Serviço");
        all.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task ExistsWithNameAsync_ShouldRespectExcludeId()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        _dbContext!.ServiceCategories.Add(category);
        
        var service = new ServiceBuilder().WithName("Teste").WithCategoryId(category.Id).Build();
        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        var exists = await _queries!.ExistsWithNameAsync("Teste", excludeId: service.Id, categoryId: null);
        exists.Should().BeFalse();
        
        var existsWithDifferentId = await _queries.ExistsWithNameAsync("Teste", excludeId: ServiceId.From(Guid.NewGuid()), categoryId: null);
        existsWithDifferentId.Should().BeTrue();
    }
}
