using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.ModuleApi;

/// <summary>
/// Interface da API pública do módulo Providers para outros módulos
/// </summary>
public interface IProvidersModuleApi : IModuleApi
{
    /// <summary>
    /// Obtém informações de um provider por ID
    /// </summary>
    Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações de um provider por documento
    /// </summary>
    Task<Result<ModuleProviderDto?>> GetProviderByDocumentAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações básicas de múltiplos providers por IDs
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBasicInfoAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações de um provider por ID do usuário
    /// </summary>
    Task<Result<ModuleProviderDto?>> GetProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por lote de critérios
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBatchAsync(IEnumerable<Guid> providerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um provider existe
    /// </summary>
    Task<Result<bool>> ProviderExistsAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário é um provider
    /// </summary>
    Task<Result<bool>> UserIsProviderAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um documento existe
    /// </summary>
    Task<Result<bool>> DocumentExistsAsync(string document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por cidade
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por estado
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(string state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por tipo
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(string providerType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém providers por status de verificação
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(string verificationStatus, CancellationToken cancellationToken = default);
}
