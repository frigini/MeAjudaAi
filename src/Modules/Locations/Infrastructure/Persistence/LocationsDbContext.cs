using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo Locations.
/// Gerencia cidades permitidas e dados de validação geográfica.
/// </summary>
public class LocationsDbContext : BaseDbContext
{
    public DbSet<AllowedCity> AllowedCities => Set<AllowedCity>();

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="LocationsDbContext"/> para operações design-time (migrações).
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    public LocationsDbContext(DbContextOptions<LocationsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="LocationsDbContext"/> para runtime com injeção de dependência.
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    /// <param name="domainEventProcessor">O processador de eventos de domínio.</param>
    public LocationsDbContext(DbContextOptions<LocationsDbContext> options, IDomainEventProcessor domainEventProcessor) : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define schema padrão para este módulo
        modelBuilder.HasDefaultSchema("locations");

        // Aplica todas as configurações do assembly atual
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocationsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Suprime warning de modelo pendente em ambiente de teste
        // Isso é necessário porque ambientes de teste podem ter configurações ligeiramente diferentes
        var isTestEnvironment = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "true";
        if (isTestEnvironment)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        // Módulo Locations atualmente não possui entidades com eventos de domínio
        // AllowedCity é uma entidade CRUD simples sem eventos de negócio
        return Task.FromResult(new List<IDomainEvent>());
    }

    protected override void ClearDomainEvents()
    {
        // Nenhum evento de domínio para limpar no módulo Locations
    }
}
