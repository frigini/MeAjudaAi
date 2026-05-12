using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace MeAjudaAi.Shared.Database;

public abstract class BaseDbContext : DbContext, IRepository<OutboxMessage, Guid>
{
    private readonly IDomainEventProcessor? _domainEventProcessor;

    static BaseDbContext()
    {
        try { System.IO.File.WriteAllText(@"C:\Code\MeAjudaAi\tests\MeAjudaAi.E2E.Tests\bin\Debug\net10.0\base_db_loaded.txt", "Loaded at " + System.DateTime.UtcNow.ToString("O")); } catch { }
    }

    protected BaseDbContext(DbContextOptions options) : base(options)
    {
        _domainEventProcessor = null;
    }

    protected BaseDbContext(DbContextOptions options, IDomainEventProcessor domainEventProcessor) : base(options)
    {
        _domainEventProcessor = domainEventProcessor;
    }

    #region IRepository<OutboxMessage, Guid>

    public async Task<OutboxMessage?> TryFindAsync(Guid key, CancellationToken cancellationToken)
    {
        return await Set<OutboxMessage>().FirstOrDefaultAsync(x => x.Id == key, cancellationToken);
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
        var diagPath = @"C:\Code\MeAjudaAi\tests\MeAjudaAi.E2E.Tests\bin\Debug\net10.0\db_diag.log";
        await AppendLogAsync(diagPath, $"[DB] SaveChangesAsync starting for {GetType().Name}...");
        
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
        await AppendLogAsync(diagPath, $"[DB] Calling base.SaveChangesAsync for {GetType().Name}...");
        var result = await base.SaveChangesAsync(cancellationToken);
        await AppendLogAsync(diagPath, $"[DB] base.SaveChangesAsync completed for {GetType().Name}. RowCount: {result}");
 
        // 4. Processar eventos de domínio APÓS salvar (fora da transação)
        if (domainEvents.Any())
        {
            await AppendLogAsync(diagPath, $"[DB] Processing {domainEvents.Count} domain events for {GetType().Name}...");
            await _domainEventProcessor.ProcessDomainEventsAsync(domainEvents, cancellationToken);
            await AppendLogAsync(diagPath, $"[DB] Domain events processed for {GetType().Name}.");
        }
 
        return result;
    }

    private async Task AppendLogAsync(string path, string message)
    {
        try
        {
            await File.AppendAllTextAsync(path, $"[{DateTime.UtcNow:O}] {message}\n");
        }
        catch { /* ignored */ }
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
