using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Domain.Repositories;

/// <summary>
/// Repositório para operações de persistência de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa o padrão Repository para encapsular a lógica de acesso a dados
/// e manter a separação entre o domínio e a infraestrutura.
/// </remarks>
public interface IProviderRepository
{
    /// <summary>
    /// Busca um prestador de serviços pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>O prestador encontrado ou null se não existir</returns>
    Task<Provider?> GetByIdAsync(ProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca múltiplos prestadores de serviços pelos seus identificadores únicos.
    /// </summary>
    /// <param name="ids">Lista de identificadores dos prestadores</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista de prestadores encontrados</returns>
    Task<IReadOnlyList<Provider>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo prestador de serviços ao repositório.
    /// </summary>
    /// <param name="provider">Prestador a ser adicionado</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    Task AddAsync(Provider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um prestador de serviços existente no repositório.
    /// </summary>
    /// <param name="provider">Prestador com dados atualizados</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um prestador de serviços do repositório (exclusão física).
    /// </summary>
    /// <param name="id">Identificador do prestador a ser removido</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <remarks>
    /// Esta operação realiza exclusão física. Para exclusão lógica, use o método Delete da entidade Provider.
    /// </remarks>
    Task DeleteAsync(ProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um prestador de serviços existe no repositório.
    /// </summary>
    /// <param name="id">Identificador do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>True se o prestador existir, false caso contrário</returns>
    Task<bool> ExistsAsync(ProviderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um prestador de serviços pelo ID do usuário.
    /// </summary>
    /// <param name="userId">ID do usuário no Keycloak</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Prestador de serviços encontrado ou null se não existir</returns>
    Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um prestador de serviços por documento (CPF, CNPJ, etc.).
    /// </summary>
    /// <param name="document">Número do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Prestador de serviços encontrado ou null se não existir</returns>
    Task<Provider?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Obtém o status de verificação de um prestador de serviços sem carregar a entidade completa.
    /// </summary>
    /// <param name="id">Identificador do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Uma tupla contendo:
    /// - Exists: indica se o prestador existe
    /// - Status: o status de verificação do prestador (null se não existir)
    /// </returns>
    /// <remarks>
    /// Este método é otimizado para consultas leves que precisam apenas do status,
    /// evitando carregar a entidade completa com todas as suas relações.
    /// </remarks>
    Task<(bool Exists, EVerificationStatus? Status)> GetProviderStatusAsync(
        ProviderId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existem prestadores de serviços que oferecem um serviço específico.
    /// </summary>
    /// <param name="serviceId">Identificador do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>True se existir ao menos um prestador oferecendo o serviço, False caso contrário</returns>
    Task<bool> HasProvidersWithServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
