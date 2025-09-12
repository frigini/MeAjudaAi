using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public async Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<User>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return await Task.FromResult(domainEvents);
    }

    public void ClearDomainEvents()
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