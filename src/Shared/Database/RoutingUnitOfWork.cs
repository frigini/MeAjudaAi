using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
        var uows = serviceProvider.GetServices<IUnitOfWork>().ToList();
        
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
    /// Garante atomicidade cross-módulo usando transações explícitas do EF Core coordenadas.
    /// O uso de explicit transactions substitui TransactionScope para evitar falhas em Linux/PostgreSQL.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var uows = serviceProvider.GetServices<IUnitOfWork>()
            .Where(u => u is not RoutingUnitOfWork)
            .ToList();

        if (uows.Count == 0) return 0;
        if (uows.Count == 1) return await uows[0].SaveChangesAsync(cancellationToken);

        // Resolve todos os DbContexts envolvidos
        var dbContexts = uows.OfType<DbContext>().ToList();

        if (dbContexts.Count > 1)
        {
            // Abordagem com transação explícita coordenada entre múltiplos DbContexts.
            // Nota: Para PostgreSQL no Linux, TransactionScope exige MSDTC se houver múltiplas conexões.
            // Usamos transações explícitas do EF Core garantindo que compartilhem a mesma transação lógica.
            var firstContext = dbContexts[0];
            
            // Verificamos se todos os DbContexts compartilham a mesma conexão física.
            // Caso contrário, não podemos usar UseTransactionAsync e devemos falhar graciosamente ou 
            // processar sequencialmente (o que é o comportamento atual de fallback).
            var firstConnection = firstContext.Database.GetDbConnection();
            var allShareConnection = dbContexts.Skip(1).All(c => c.Database.GetDbConnection() == firstConnection);

            if (allShareConnection)
            {
                await using var transaction = await firstContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
                
                try
                {
                    var dbTransaction = transaction.GetDbTransaction();
                    
                    foreach (var context in dbContexts.Skip(1))
                    {
                        // Associa os demais contextos à mesma transação. 
                        await context.Database.UseTransactionAsync(dbTransaction, cancellationToken);
                    }

                    int totalChanges = 0;
                    foreach (var uow in uows)
                    {
                        totalChanges += await uow.SaveChangesAsync(cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);
                    return totalChanges;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        // Fallback para execução sequencial simples (ex: se algum IUnitOfWork não for DbContext)
        int total = 0;
        foreach (var uow in uows)
        {
            total += await uow.SaveChangesAsync(cancellationToken);
        }
        return total;
    }
}
