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
    private IReadOnlyDictionary<(Type aggregate, Type key), IUnitOfWork>? _aggregateToUow;

    private IReadOnlyDictionary<(Type aggregate, Type key), IUnitOfWork> GetAggregateMap()
    {
        if (_aggregateToUow is null)
        {
            var uows = serviceProvider.GetServices<IUnitOfWork>().Where(u => u is not RoutingUnitOfWork).ToList();
            var map = new Dictionary<(Type aggregate, Type key), IUnitOfWork>();
            foreach (var uow in uows)
            {
                foreach (var repoInterface in uow.GetType().GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<,>)))
                {
                    var aggregateType = repoInterface.GetGenericArguments()[0];
                    var keyType = repoInterface.GetGenericArguments()[1];
                    var pair = (aggregateType, keyType);
                    if (map.TryGetValue(pair, out var existing))
                    {
                        throw new InvalidOperationException(
                            $"Ambiguous Unit of Work routing for aggregate {aggregateType.Name} with key {keyType.Name}. " +
                            $"Both {existing.GetType().Name} and {uow.GetType().Name} implement IRepository<{aggregateType.Name}, {keyType.Name}>. " +
                            "Ensure only one DbContext implements IRepository for each (aggregate, key) pair.");
                    }
                    map[pair] = uow;
                }
            }
            _aggregateToUow = map;
        }
        return _aggregateToUow;
    }

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        var map = GetAggregateMap();
        var pair = (typeof(TAggregate), typeof(TKey));
        if (map.TryGetValue(pair, out var uow))
        {
            if (uow is not IRepository<TAggregate, TKey> repo)
            {
                throw new InvalidOperationException(
                    $"Unit of Work {uow.GetType().Name} is registered for {typeof(TAggregate).Name}<{typeof(TKey).Name}> but does not implement IRepository<{typeof(TAggregate).Name}, {typeof(TKey).Name}>.");
            }
            return repo;
        }

        var hasAggregate = map.Keys.Any(k => k.aggregate == typeof(TAggregate));
        if (hasAggregate)
        {
            var matching = map.Keys.Where(k => k.aggregate == typeof(TAggregate)).Select(k => k.key).ToList();
            throw new InvalidOperationException(
                $"No Unit of Work found for {typeof(TAggregate).Name}<{typeof(TKey).Name}>. " +
                $"Found implementations for {typeof(TAggregate).Name} with key types: {string.Join(", ", matching.Select(t => t.Name))}. " +
                "Ensure the correct key type is used or only one DbContext implements IRepository for this aggregate.");
        }

        throw new InvalidOperationException(
            $"No Unit of Work (DbContext) found that supports aggregate {typeof(TAggregate).Name}<{typeof(TKey).Name}>. " +
            "Ensure the corresponding module is registered and its DbContext implements IRepository for this aggregate.");
    }

    /// <summary>
    /// Salva as alterações em todas as Unidades de Trabalho registradas.
    /// Este método itera sobre todos os IUnitOfWork registrados (exceto RoutingUnitOfWork)
    /// e chama SaveChangesAsync de cada um em transações separadas. Falhas em UoWs
    /// posteriores NÃO fazem rollback das anteriores — não há garantia de atomicidade
    /// cross-módulo. Para integrações que exigem consistência entre módulos, use o
    /// padrão Outbox para garantir eventual consistência.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var uows = serviceProvider.GetServices<IUnitOfWork>()
            .Where(u => u is not RoutingUnitOfWork)
            .ToList();

        if (uows.Count == 0) return 0;
        if (uows.Count == 1) return await uows[0].SaveChangesAsync(cancellationToken);

        int total = 0;
        foreach (var uow in uows)
        {
            total += await uow.SaveChangesAsync(cancellationToken);
        }
        return total;
    }
}
