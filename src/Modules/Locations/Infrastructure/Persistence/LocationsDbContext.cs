using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence;

/// <summary>
/// Database context for the Locations module.
/// Manages allowed cities and geographic validation data.
/// </summary>
public class LocationsDbContext : BaseDbContext
{
    public DbSet<AllowedCity> AllowedCities => Set<AllowedCity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationsDbContext"/> class for design-time operations (migrations).
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public LocationsDbContext(DbContextOptions<LocationsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationsDbContext"/> class for runtime with dependency injection.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <param name="domainEventProcessor">The domain event processor.</param>
    public LocationsDbContext(DbContextOptions<LocationsDbContext> options, IDomainEventProcessor domainEventProcessor) : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for this module
        modelBuilder.HasDefaultSchema("locations");

        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocationsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Suppress pending model changes warning in test environment
        // This is needed because test environments may have slightly different configurations
        var isTestEnvironment = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "true";
        if (isTestEnvironment)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        // Locations module currently has no entities with domain events
        // AllowedCity is a simple CRUD entity without business events
        return Task.FromResult(new List<IDomainEvent>());
    }

    protected override void ClearDomainEvents()
    {
        // No domain events to clear in Locations module
    }
}
