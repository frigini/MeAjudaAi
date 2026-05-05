using System.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Unidade de Trabalho que roteia operações para as implementações específicas de cada módulo.
/// Em um monolito modular, cada módulo possui seu próprio DbContext (que implementa IUnitOfWork).
/// Esta classe permite que a camada de aplicação continue usando a interface genérica IUnitOfWork.
/// </summary>
public class RoutingUnitOfWork(IServiceProvider serviceProvider) : IUnitOfWork
{
    private IUnitOfWork? _activeUow;

    /// <summary>
    /// Obtém o repositório correto procurando entre todas as implementações de IUnitOfWork registradas.
    /// Caso mais de uma implementação suporte o agregado (ex: OutboxMessage), tenta resolver a ambiguidade
    /// preferindo a Unidade de Trabalho que já está ativa no escopo atual.
    /// </summary>
    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        // Resolve todas as implementações de IUnitOfWork registradas no DI
        var uows = serviceProvider.GetServices<IUnitOfWork>();
        var matches = uows.Where(u => u is not RoutingUnitOfWork && u is IRepository<TAggregate, TKey>).ToList();

        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"No Unit of Work (DbContext) found that supports aggregate {typeof(TAggregate).Name}. " +
                "Ensure the corresponding module is registered and its DbContext implements IRepository for this aggregate.");
        }

        if (matches.Count > 1)
        {
            // Tenta resolver ambiguidade preferindo a UoW já ativa no escopo
            if (_activeUow != null && matches.Contains(_activeUow))
            {
                return (IRepository<TAggregate, TKey>)_activeUow;
            }

            var types = string.Join(", ", matches.Select(m => m.GetType().Name));
            throw new InvalidOperationException(
                $"Ambiguous Unit of Work routing for aggregate {typeof(TAggregate).Name}. " +
                $"Multiple implementations found: {types}. Please use a module-specific marker interface or ensure only the correct DbContext implements IRepository for this type.");
        }

        var match = matches[0];
        _activeUow = match;
        return (IRepository<TAggregate, TKey>)match;
    }

    /// <summary>
    /// Salva as alterações em todas as Unidades de Trabalho registradas.
    /// Garante atomicidade cross-módulo usando TransactionScope com IsolationLevel.ReadCommitted.
    /// Em um fluxo de comando típico, apenas uma Unidade de Trabalho terá mudanças pendentes.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var uows = serviceProvider.GetServices<IUnitOfWork>()
            .Where(u => u is not RoutingUnitOfWork)
            .ToList();

        if (uows.Count == 0) return 0;
        if (uows.Count == 1) return await uows[0].SaveChangesAsync(cancellationToken);

        // Para múltiplos DbContexts, usamos TransactionScope para garantir atomicidade.
        // Importante: Usamos ReadCommitted para evitar deadlocks comuns com o padrão Serializable.
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout
        };

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            transactionOptions,
            TransactionScopeAsyncFlowOption.Enabled);
        
        int totalChanges = 0;
        foreach (var uow in uows)
        {
            totalChanges += await uow.SaveChangesAsync(cancellationToken);
        }

        scope.Complete();
        
        return totalChanges;
    }
}
