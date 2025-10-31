using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.Providers;

/// <summary>
/// API pública do módulo Providers para consumo por outros módulos
/// </summary>
public interface IProvidersModuleApi
{
    /// <summary>
    /// Obtém dados básicos de um prestador por ID
    /// </summary>
    Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém dados básicos de um prestador por ID do usuário
    /// </summary>
    Task<Result<ModuleProviderDto?>> GetProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações básicas de múltiplos prestadores
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBatchAsync(IReadOnlyList<Guid> providerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um prestador existe
    /// </summary>
    Task<Result<bool>> ProviderExistsAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário já é prestador
    /// </summary>
    Task<Result<bool>> UserIsProviderAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém prestadores por cidade
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByCityAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém prestadores por estado
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByStateAsync(string state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém prestadores por tipo
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByTypeAsync(EProviderType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém prestadores por status de verificação
    /// </summary>
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersByVerificationStatusAsync(EVerificationStatus status, CancellationToken cancellationToken = default);
}