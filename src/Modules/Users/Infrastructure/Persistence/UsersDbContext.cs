using System.Reflection;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados Entity Framework Core para o módulo Users.
/// Gerencia entidades de usuário e aplica configurações específicas do módulo.
/// </summary>
public class UsersDbContext : BaseDbContext
{
    /// <summary>
    /// Obtém o conjunto de entidades Users para consulta e persistência de entidades User.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="UsersDbContext"/> para operações de design-time (migrations).
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="UsersDbContext"/> para runtime com injeção de dependência.
    /// </summary>
    /// <param name="options">As opções a serem usadas pelo DbContext.</param>
    /// <param name="domainEventProcessor">O processador de eventos de domínio para manipulação de eventos de domínio.</param>
    public UsersDbContext(DbContextOptions<UsersDbContext> options, IDomainEventProcessor domainEventProcessor) : base(options, domainEventProcessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<User>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
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
