using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.Search.Infrastructure.Persistence;

/// <summary>
/// Fábrica para criar instâncias de SearchDbContext em tempo de design (para migrações).
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
            npgsqlOptions.UseNetTopologySuite(); // Habilitar suporte PostGIS
        });

        optionsBuilder.UseSnakeCaseNamingConvention();

        // Criar processador de eventos de domínio no-op para design-time
        var domainEventProcessor = new NoOpDomainEventProcessor();

        return new SearchDbContext(optionsBuilder.Options, domainEventProcessor);
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
