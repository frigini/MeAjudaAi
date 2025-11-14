using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.Search.Infrastructure.Persistence;

/// <summary>
/// Factory for creating SearchDbContext instances at design-time (for migrations).
/// </summary>
public class SearchDbContextFactory : IDesignTimeDbContextFactory<SearchDbContext>
{
    public SearchDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SearchDbContext>();

        // Read connection string from environment variable
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=MeAjudaAi;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search");
            npgsqlOptions.UseNetTopologySuite(); // Enable PostGIS support
        });

        optionsBuilder.UseSnakeCaseNamingConvention();

        // Create a no-op domain event processor for design-time
        var noOpEventProcessor = new NoOpDomainEventProcessor();

        return new SearchDbContext(optionsBuilder.Options, noOpEventProcessor);
    }

    /// <summary>
    /// No-op implementation of IDomainEventProcessor for design-time scenarios.
    /// </summary>
    private class NoOpDomainEventProcessor : IDomainEventProcessor
    {
        public Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
