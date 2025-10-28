using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Database;

namespace MeAjudaAi.Modules.Providers.Domain.Repositories;

/// <summary>
/// Repositório para operações de persistência de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa o padrão Repository para encapsular a lógica de acesso a dados
/// e manter a separação entre o domínio e a infraestrutura.
/// </remarks>
public interface IProviderRepository : IRepository<Provider, ProviderId>
{
    /// <summary>
    /// Busca um prestador de serviços pelo ID do usuário.
    /// </summary>
    /// <param name="userId">ID do usuário no Keycloak</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Prestador de serviços encontrado ou null se não existir</returns>
    Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um prestador de serviços para o usuário especificado.
    /// </summary>
    /// <param name="userId">ID do usuário no Keycloak</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe, False caso contrário</returns>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores de serviços por cidade.
    /// </summary>
    /// <param name="city">Nome da cidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de prestadores de serviços na cidade</returns>
    Task<IReadOnlyList<Provider>> GetByCityAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores de serviços por estado.
    /// </summary>
    /// <param name="state">Nome do estado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de prestadores de serviços no estado</returns>
    Task<IReadOnlyList<Provider>> GetByStateAsync(string state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores de serviços por status de verificação.
    /// </summary>
    /// <param name="verificationStatus">Status de verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de prestadores de serviços com o status especificado</returns>
    Task<IReadOnlyList<Provider>> GetByVerificationStatusAsync(
        EVerificationStatus verificationStatus, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca prestadores de serviços por tipo.
    /// </summary>
    /// <param name="type">Tipo do prestador de serviços</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de prestadores de serviços do tipo especificado</returns>
    Task<IReadOnlyList<Provider>> GetByTypeAsync(
        EProviderType type, 
        CancellationToken cancellationToken = default);
}
