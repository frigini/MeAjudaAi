using System.Reflection;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Users module.
/// Manages user entities and applies module-specific database configurations.
/// </summary>
public class UsersDbContext : BaseDbContext
{
    /// <summary>
    /// Gets the Users entity set for querying and saving User entities.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersDbContext"/> class for design-time operations (migrations).
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersDbContext"/> class for runtime with dependency injection.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <param name="domainEventProcessor">The domain event processor for handling domain events.</param>
    public UsersDbContext(DbContextOptions<UsersDbContext> options, IDomainEventProcessor domainEventProcessor) : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("meajudaai_users");

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override async Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<User>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return await Task.FromResult(domainEvents);
    }

    protected override void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<User>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
