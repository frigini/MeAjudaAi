using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

/// <summary>
/// Interface do serviço de integração com Keycloak para gerenciamento de identidade.
/// </summary>
/// <remarks>
/// Define contratos para operações de autenticação, autorização e gerenciamento
/// de usuários no Keycloak. Abstrai a complexidade da comunicação com APIs REST
/// do Keycloak, fornecendo interface limpa para operações de identidade.
/// </remarks>
public interface IKeycloakService
{
    /// <summary>
    /// Cria um novo usuário no Keycloak.
    /// </summary>
    /// <param name="username">Nome de usuário único</param>
    /// <param name="email">Endereço de email único</param>
    /// <param name="firstName">Primeiro nome do usuário</param>
    /// <param name="lastName">Sobrenome do usuário</param>
    /// <param name="password">Senha inicial do usuário</param>
    /// <param name="roles">Papéis/funções a serem atribuídas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>
    /// Resultado contendo:
    /// - Sucesso: ID único do usuário criado no Keycloak
    /// - Falha: Mensagem de erro detalhada
    /// </returns>
    Task<Result<string>> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Autentica um usuário no Keycloak.
    /// </summary>
    /// <param name="usernameOrEmail">Nome de usuário ou email para autenticação</param>
    /// <param name="password">Senha do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>
    /// Resultado contendo:
    /// - Sucesso: AuthenticationResult com tokens e informações do usuário
    /// - Falha: Mensagem de erro de autenticação
    /// </returns>
    Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida um token de acesso do Keycloak.
    /// </summary>
    /// <param name="token">Token de acesso a ser validado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>
    /// Resultado contendo:
    /// - Sucesso: TokenValidationResult com informações do token
    /// - Falha: Mensagem de erro de validação
    /// </returns>
    Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa um usuário no Keycloak.
    /// </summary>
    /// <param name="keycloakId">ID único do usuário no Keycloak</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>
    /// Resultado da operação:
    /// - Sucesso: Usuário desativado com sucesso
    /// - Falha: Mensagem de erro da operação
    /// </returns>
    /// <remarks>
    /// Utilizada para desativar usuários sem remover completamente
    /// suas informações do sistema de identidade.
    /// </remarks>
    Task<Result> DeactivateUserAsync(
        string keycloakId,
        CancellationToken cancellationToken = default);
}