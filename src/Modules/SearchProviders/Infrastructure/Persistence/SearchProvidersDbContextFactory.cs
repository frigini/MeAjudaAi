using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;

/// <summary>
/// Fábrica para criar instâncias de SearchProvidersDbContext em tempo de design (para migrações).
/// </summary>
public class SearchProvidersDbContextFactory : IDesignTimeDbContextFactory<SearchProvidersDbContext>
{
    public SearchProvidersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SearchProvidersDbContext>();

        // Read connection string from environment variable
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=MeAjudaAi;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search_providers");
            npgsqlOptions.UseNetTopologySuite(); // Habilitar suporte PostGIS
        });

        optionsBuilder.UseSnakeCaseNamingConvention();

        // Criar processador de eventos de domínio no-op para design-time
        var domainEventProcessor = new NoOpDomainEventProcessor();

        return new SearchProvidersDbContext(optionsBuilder.Options, domainEventProcessor);
    }

    /// <summary>
    /// Implementação no-op de IDomainEventProcessor para cenários de tempo de design.
    /// </summary>
    private class NoOpDomainEventProcessor : IDomainEventProcessor
    {
        public Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
