using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Shared.Events;

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

    protected abstract Task<List<IDomainEvent>> GetDomainEventsAsync(CancellationToken cancellationToken = default);
    protected abstract void ClearDomainEvents();
}