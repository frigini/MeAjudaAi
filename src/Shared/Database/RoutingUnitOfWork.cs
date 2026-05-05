using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Unidade de Trabalho que roteia operações para as implementações específicas de cada módulo.
/// Em um monolito modular, cada módulo possui seu próprio DbContext (que implementa IUnitOfWork).
/// Esta classe permite que a camada de aplicação continue usando a interface genérica IUnitOfWork.
/// </summary>
public class RoutingUnitOfWork(IServiceProvider serviceProvider) : IUnitOfWork
{
    /// <summary>
    /// Obtém o repositório correto procurando entre todas as implementações de IUnitOfWork registradas.
    /// </summary>
    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        // Resolve todas as implementações de IUnitOfWork registradas no DI
        var uows = serviceProvider.GetServices<IUnitOfWork>();
        
        foreach (var uow in uows)
        {
            // Evita recursão infinita
            if (uow is RoutingUnitOfWork) continue;
            
            // Verifica se este DbContext/UoW implementa o repositório para o agregado solicitado
            if (uow is IRepository<TAggregate, TKey> repository)
            {
                return repository;
            }
        }
        
        throw new InvalidOperationException(
            $"Nenhuma Unidade de Trabalho (DbContext) encontrada que suporte a agregação {typeof(TAggregate).Name}. " +
            "Certifique-se de que o módulo correspondente foi registrado e que seu DbContext implementa IRepository para este agregado.");
    }

    /// <summary>
    /// Salva as alterações em todas as Unidades de Trabalho registradas.
    /// Em um fluxo de comando típico, apenas uma Unidade de Trabalho terá mudanças pendentes.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var uows = serviceProvider.GetServices<IUnitOfWork>();
        int totalChanges = 0;
        
        foreach (var uow in uows)
        {
            if (uow is RoutingUnitOfWork) continue;
            
            totalChanges += await uow.SaveChangesAsync(cancellationToken);
        }
        
        return totalChanges;
    }
}
