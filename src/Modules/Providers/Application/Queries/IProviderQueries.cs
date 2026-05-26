using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Interface para consultas otimizadas de leitura (NoTracking) do módulo Providers.
/// </summary>
public interface IProviderQueries
{
    /// <summary>
    /// Busca um prestador pelo identificador único sem rastreamento.
    /// </summary>
    Task<Provider?> GetByIdAsync(ProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um prestador pelo slug amigável sem rastreamento.
    /// </summary>
    Task<Provider?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca múltiplos prestadores pelos seus identificadores únicos.
    /// </summary>
    Task<IReadOnlyList<Provider>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um prestador pelo ID do usuário associado.
    /// </summary>
    Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um prestador pelo número de documento.
    /// </summary>
    Task<Provider?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um prestador para o usuário especificado.
    /// </summary>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores por cidade.
    /// </summary>
    Task<IReadOnlyList<Provider>> GetByCityAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores por estado.
    /// </summary>
    Task<IReadOnlyList<Provider>> GetByStateAsync(string state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores pelo status de verificação.
    /// </summary>
    Task<IReadOnlyList<Provider>> GetByVerificationStatusAsync(EVerificationStatus verificationStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores por tipo.
    /// </summary>
    Task<IReadOnlyList<Provider>> GetByTypeAsync(EProviderType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o status de verificação de um prestador de forma leve.
    /// </summary>
    Task<(bool Exists, EVerificationStatus? Status)> GetProviderStatusAsync(ProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existem prestadores que oferecem um serviço específico.
    /// </summary>
    Task<bool> HasProvidersWithServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores com paginação e filtros de forma otimizada.
    /// </summary>
    Task<PagedResult<Provider>> GetPagedAsync(
        int page,
        int pageSize,
        string? nameFilter = null,
        EProviderType? typeFilter = null,
        EVerificationStatus? verificationStatusFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um prestador existe no banco de dados.
    /// </summary>
    Task<bool> ExistsAsync(ProviderId id, CancellationToken cancellationToken = default);
}
