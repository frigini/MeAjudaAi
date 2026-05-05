using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Database;

public abstract class BaseDbContext : DbContext, IRepository<OutboxMessage, Guid>
{
    private readonly IDomainEventProcessor? _domainEventProcessor;

    protected BaseDbContext(DbContextOptions options) : base(options)
    {
        _domainEventProcessor = null;
    }

    protected BaseDbContext(DbContextOptions options, IDomainEventProcessor domainEventProcessor) : base(options)
    {
        _domainEventProcessor = domainEventProcessor;
    }

    #region IRepository<OutboxMessage, Guid>

    public async Task<OutboxMessage?> TryFindAsync(Guid key, CancellationToken ct)
    {
        return await Set<OutboxMessage>().FirstOrDefaultAsync(x => x.Id == key, ct);
    }

    public void Add(OutboxMessage aggregate)
    {
        Set<OutboxMessage>().Add(aggregate);
    }

    public void Delete(OutboxMessage aggregate)
    {
        Set<OutboxMessage>().Remove(aggregate);
    }

    #endregion

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Se não há domain event processor (design-time), usa comportamento padrão
        if (_domainEventProcessor == null)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        // 1. Obter eventos de domínio antes de salvar
        var domainEvents = await GetDomainEventsAsync(cancellationToken);

        // 2. Limpar eventos das entidades ANTES de salvar (para evitar reprocessamento)
        ClearDomainEvents();

        // 3. Salvar mudanças no banco
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. Processar eventos de domínio APÓS salvar (fora da transação)
        if (domainEvents.Any())
        {
            await _domainEventProcessor.ProcessDomainEventsAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    protected virtual Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<MeAjudaAi.Shared.Domain.IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        return Task.FromResult(domainEvents);
    }

    protected virtual void ClearDomainEvents()
    {
        var entries = ChangeTracker
            .Entries<MeAjudaAi.Shared.Domain.IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .ToList();

        foreach (var entry in entries)
        {
            entry.Entity.ClearDomainEvents();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
