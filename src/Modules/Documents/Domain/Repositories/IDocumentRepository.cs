using MeAjudaAi.Modules.Documents.Domain.Aggregates;

namespace MeAjudaAi.Modules.Documents.Domain.Repositories;

/// <summary>
/// Repositório para operações de persistência de documentos
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Busca um documento pelo seu identificador
    /// </summary>
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca todos os documentos de um provedor
    /// </summary>
    Task<IReadOnlyList<Document>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo documento
    /// </summary>
    Task AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um documento existente
    /// </summary>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um documento existe
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva as alterações no banco de dados
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
