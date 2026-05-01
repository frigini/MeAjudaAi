namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Define uma unidade de trabalho para gerenciar transações e operações de persistência de forma atômica.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Obtém um repositório para uma entidade de agregação específica.
    /// </summary>
    /// <typeparam name="TAggregate">O tipo da raiz da agregação.</typeparam>
    /// <typeparam name="TKey">O tipo da chave primária.</typeparam>
    /// <returns>Uma instância de IRepository para a agregação especificada.</returns>
    IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>();

    /// <summary>
    /// Persiste todas as alterações pendentes no armazenamento de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento para a operação assíncrona.</param>
    /// <returns>Uma tarefa representando a operação, contendo a contagem de entidades persistidas.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}