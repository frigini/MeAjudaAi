using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Contracts.Functional;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

/// <summary>
/// Interface Refit para consumo da API de usuários do MeAjudaAi.
/// </summary>
public interface IUsersApi
{
    // === Endpoints Públicos ===

    /// <summary>
    /// Registra um novo cliente no sistema.
    /// </summary>
    /// <param name="request">Dados do cliente a ser registrado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário criado</returns>
    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}{ApiEndpoints.Users.Register}")]
    Task<Result<ModuleUserFullDto>> RegisterCustomerAsync(
        [Body] RegisterCustomerRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista os provedores de autenticação social disponíveis.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de nomes dos provedores</returns>
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}{ApiEndpoints.Users.GetAuthProviders}")]
    Task<Result<string[]>> GetAuthProvidersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o token do dispositivo do usuário para notificações push.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="request">Dados do token do dispositivo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}{ApiEndpoints.Users.UpdateDeviceToken}")]
    Task UpdateDeviceTokenAsync(
        Guid id,
        [Body] DeviceTokenRequestDto request,
        CancellationToken cancellationToken = default);

    // === Endpoints Admin ===

    /// <summary>
    /// Cria um novo usuário no sistema (requer admin).
    /// </summary>
    /// <param name="request">Dados do usuário a ser criado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário criado</returns>
    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}")]
    Task<Result<ModuleUserFullDto>> CreateUserAsync(
        [Body] CreateUserRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui um usuário do sistema (requer admin).
    /// </summary>
    /// <param name="id">ID do usuário a ser excluído</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Delete($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}{ApiEndpoints.Users.Delete}")]
    Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo email (requer admin).
    /// </summary>
    /// <param name="email">Email do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário encontrado</returns>
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}{ApiEndpoints.Users.GetByEmail}")]
    Task<Result<ModuleUserFullDto>> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo ID.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário encontrado</returns>
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}{ApiEndpoints.Users.GetById}")]
    Task<Result<ModuleUserFullDto>> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista usuários com paginação e busca (requer admin).
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <param name="searchTerm">Termo de busca opcional</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de usuários</returns>
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}")]
    Task<Result<PagedResult<ModuleUserFullDto>>> GetUsersAsync(
        [Query] int pageNumber = 1,
        [Query] int pageSize = 10,
        [Query] string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o perfil de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="request">Dados atualizados do perfil</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do usuário atualizado</returns>
    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Users.Base}{ApiEndpoints.Users.UpdateProfile}")]
    Task<Result<ModuleUserFullDto>> UpdateUserProfileAsync(
        Guid id,
        [Body] UpdateUserProfileRequestDto request,
        CancellationToken cancellationToken = default);
}
