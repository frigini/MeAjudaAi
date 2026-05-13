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
    private IReadOnlyDictionary<Type, IUnitOfWork>? _aggregateToUow;

    private IReadOnlyDictionary<Type, IUnitOfWork> GetAggregateMap()
    {
        if (_aggregateToUow is null)
        {
            var uows = serviceProvider.GetServices<IUnitOfWork>().Where(u => u is not RoutingUnitOfWork).ToList();
            var map = new Dictionary<Type, IUnitOfWork>();
            foreach (var uow in uows)
            {
                foreach (var aggregateType in uow.GetType().GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<,>))
                    .Select(i => i.GetGenericArguments()[0]))
                {
                    if (map.TryGetValue(aggregateType, out var existing))
                    {
                        throw new InvalidOperationException(
                            $"Ambiguous Unit of Work routing for aggregate {aggregateType.Name}. " +
                            $"Both {existing.GetType().Name} and {uow.GetType().Name} implement IRepository for this type. " +
                            "Ensure only one DbContext implements IRepository for each aggregate.");
                    }
                    map[aggregateType] = uow;
                }
            }
            _aggregateToUow = map;
        }
        return _aggregateToUow;
    }

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        var map = GetAggregateMap();
        if (map.TryGetValue(typeof(TAggregate), out var uow))
        {
            return (IRepository<TAggregate, TKey>)uow;
        }

        throw new InvalidOperationException(
            $"No Unit of Work (DbContext) found that supports aggregate {typeof(TAggregate).Name}. " +
            "Ensure the corresponding module is registered and its DbContext implements IRepository for this aggregate.");
    }

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
