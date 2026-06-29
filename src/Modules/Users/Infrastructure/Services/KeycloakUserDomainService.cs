using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de domínio para usuários integrada com Keycloak.
/// </summary>
/// <remarks>
/// Implementa IUserDomainService fornecendo funcionalidades de criação e sincronização
/// de usuários com integração total ao Keycloak como provedor de identidade.
/// Encapsula a complexidade de comunicação com APIs externas e mantém a consistência
/// entre o domínio local e o sistema de autenticação.
/// </remarks>
/// <param name="keycloakService">Serviço de integração com Keycloak</param>
/// <param name="logger">Logger para registro de operações e erros</param>
internal class KeycloakUserDomainService(
    IKeycloakService keycloakService,
    ILogger<KeycloakUserDomainService> logger) : IUserDomainService
{
    /// <summary>
    /// Cria um novo usuário com sincronização automática no Keycloak.
    /// </summary>
    /// <param name="username">Nome de usuário único validado</param>
    /// <param name="email">Email único validado</param>
    /// <param name="firstName">Primeiro nome do usuário</param>
    /// <param name="lastName">Sobrenome do usuário</param>
    /// <param name="password">Senha para autenticação</param>
    /// <param name="roles">Papéis/funções a serem atribuídas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>
    /// Resultado contendo:
    /// - Sucesso: Entidade User com ID do Keycloak sincronizado
    /// - Falha: Erro da criação no Keycloak ou validação
    /// </returns>
    /// <remarks>
    /// Processo de criação:
    /// 1. Envia dados para criação no Keycloak
    /// 2. Recebe ID único do Keycloak
    /// 3. Cria entidade User local com ID sincronizado
    /// 4. Retorna usuário pronto para persistência
    /// </remarks>
    /// <param name="phoneNumber">Número de telefone opcional do usuário</param>
    public async Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        // Cria o usuário no Keycloak primeiro
        var keycloakResult = await keycloakService.CreateUserAsync(
            username.Value, email.Value, firstName, lastName, password, roles, cancellationToken);

        if (keycloakResult.IsFailure)
            return Result<User>.Failure(keycloakResult.Error);

        // Cria a entidade User local com o ID retornado pelo Keycloak
        var keycloakId = keycloakResult.Value!;
        var userResult = User.Create(username, email, firstName, lastName, keycloakId, phoneNumber);
        if (userResult.IsFailure)
        {
            var deactivationResult = await keycloakService.DeactivateUserAsync(keycloakId, CancellationToken.None);
            if (deactivationResult.IsFailure)
            {
                logger.LogWarning("Failed to deactivate Keycloak user {KeycloakId} during compensation for local user creation failure. Error: {Error}", keycloakId, deactivationResult.Error.Message);
                // Silenciar falhas de compensação para evitar mascarar o erro de validação original
            }
            return Result<User>.Failure(userResult.Error);
        }
        
        return Result<User>.Success(userResult.Value!);
    }

    /// <summary>
    /// Sincroniza dados do usuário local com o Keycloak.
    /// </summary>
    /// <param name="userId">Identificador do usuário para sincronização</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>
    /// Resultado da operação de sincronização:
    /// - Sucesso: Dados sincronizados com sucesso
    /// - Falha: Erro durante a sincronização
    /// </returns>
    /// <remarks>
    /// Implementação para sincronização de dados do usuário com Keycloak.
    /// Atualmente retorna sucesso imediato. Pode ser expandida para incluir
    /// validação de existência no Keycloak, atualização de perfil, etc.
    /// </remarks>
    public Task<Result> SyncUserWithKeycloakAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("SyncUserWithKeycloakAsync called for user {UserId} - not yet implemented, returning success", userId.Value);
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// Desativa o usuário no Keycloak para compensar falha na persistência local.
    /// </summary>
    /// <param name="userId">O identificador do usuário (ID local/Keycloak).</param>
    /// <param name="cancellationToken">Token de cancelamento opcional.</param>
    /// <returns>Um Result indicando o sucesso ou falha da operação obtido do serviço Keycloak.</returns>
    public async Task<Result> DeactivateUserInKeycloakAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        // O ID do usuário local é o mesmo ID do Keycloak nesta implementação
        var keycloakId = userId.Value.ToString();
        logger.LogWarning("Deactivating Keycloak user {UserId} due to local repository failure", keycloakId);
        
        var result = await keycloakService.DeactivateUserAsync(keycloakId, CancellationToken.None);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to deactivate Keycloak user {UserId} due to local repository failure. Error: {Error}", keycloakId, result.Error.Message);
            // Silenciar falhas de compensação para evitar mascarar o erro de validação original
        }
        
        return result;
    }
}
