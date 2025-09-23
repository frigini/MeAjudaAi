using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Domain.Services;

/// <summary>
/// Interface do serviço de domínio responsável por operações complexas de usuário.
/// </summary>
/// <remarks>
/// Define contratos para operações de domínio que envolvem múltiplas entidades,
/// validações complexas de negócio ou integração com sistemas externos como Keycloak.
/// Implementa padrões DDD para encapsular lógica de negócio que não pertence
/// diretamente às entidades ou value objects.
/// </remarks>
public interface IUserDomainService
{
    /// <summary>
    /// Cria um novo usuário com integração ao Keycloak.
    /// </summary>
    /// <param name="username">Nome de usuário único no sistema</param>
    /// <param name="email">Endereço de email válido e único</param>
    /// <param name="firstName">Primeiro nome do usuário</param>
    /// <param name="lastName">Sobrenome do usuário</param>
    /// <param name="password">Senha do usuário para autenticação</param>
    /// <param name="roles">Coleção de papéis/funções atribuídas ao usuário</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: Entidade User criada e sincronizada com Keycloak
    /// - Falha: Mensagem de erro descritiva
    /// </returns>
    /// <remarks>
    /// Esta operação realiza:
    /// 1. Validações de negócio para criação de usuário
    /// 2. Criação do usuário no Keycloak
    /// 3. Sincronização das informações entre sistemas
    /// 4. Aplicação de papéis e permissões
    /// </remarks>
    Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sincroniza dados do usuário com o Keycloak.
    /// </summary>
    /// <param name="userId">Identificador único do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação indicando:
    /// - Sucesso: Sincronização realizada com sucesso
    /// - Falha: Mensagem de erro descritiva
    /// </returns>
    /// <remarks>
    /// Utilizada para:
    /// 1. Atualizar informações do usuário no Keycloak
    /// 2. Sincronizar papéis e permissões
    /// 3. Desativar usuários excluídos
    /// 4. Manter consistência entre os sistemas
    /// </remarks>
    Task<Result> SyncUserWithKeycloakAsync(
        UserId userId,
        CancellationToken cancellationToken = default);
}