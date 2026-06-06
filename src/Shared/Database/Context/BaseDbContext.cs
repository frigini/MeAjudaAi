using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Database;

public abstract class BaseDbContext : DbContext
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Obter eventos de domínio antes de salvar
        var domainEvents = await GetDomainEventsAsync(cancellationToken);

        // 3. Salvar mudanças no banco
        var result = await base.SaveChangesAsync(cancellationToken);

        // 2. Limpar eventos das entidades APÓS salvar
        ClearDomainEvents();

        // 4. Processar eventos de domínio APÓS salvar (fora da transação)
        if (_domainEventProcessor != null && domainEvents.Any())
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
}