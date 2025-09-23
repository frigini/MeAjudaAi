using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Functional;

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
internal class KeycloakUserDomainService(IKeycloakService keycloakService) : IUserDomainService
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
    public async Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        // Cria o usuário no Keycloak primeiro
        var keycloakResult = await keycloakService.CreateUserAsync(
            username.Value, email.Value, firstName, lastName, password, roles, cancellationToken);

        if (keycloakResult.IsFailure)
            return Result<User>.Failure(keycloakResult.Error);

        // Cria a entidade User local com o ID retornado pelo Keycloak
        var user = new User(username, email, firstName, lastName, keycloakResult.Value);
        return Result<User>.Success(user);
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
    /// Pode incluir: desativação de usuário, atualização de papéis, etc.
    /// Atualmente implementação placeholder - deve ser expandida conforme necessidades.
    /// </remarks>
    public async Task<Result> SyncUserWithKeycloakAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        // Implementação para sincronização de dados do usuário com Keycloak
        // Pode incluir: desativação de usuário, atualização de papéis, etc.
        await Task.CompletedTask;
        return Result.Success();
    }
}