using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.Exceptions;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Users.Domain.Entities;

/// <summary>
/// Representa um usuário do sistema como raiz de agregado.
/// Implementa o padrão Domain-Driven Design com eventos de domínio e value objects.
/// </summary>
/// <remarks>
/// Esta classe encapsula todas as regras de negócio relacionadas aos usuários,
/// incluindo registro, atualização de perfil e exclusão lógica.
/// Integra-se com o Keycloak para autenticação externa.
/// </remarks>
public sealed class User : AggregateRoot<UserId>
{
    /// <summary>
    /// Nome de usuário único no sistema.
    /// </summary>
    /// <remarks>
    /// Implementado como value object com validações específicas.
    /// </remarks>
    public Username Username { get; private set; } = null!;

    /// <summary>
    /// Endereço de email do usuário.
    /// </summary>
    /// <remarks>
    /// Implementado como value object com validação de formato de email.
    /// Deve ser único no sistema.
    /// </remarks>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Primeiro nome do usuário.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Sobrenome do usuário.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Identificador único do usuário no Keycloak (sistema de autenticação externo).
    /// </summary>
    /// <remarks>
    /// Este campo é usado para integração com o provedor de identidade Keycloak.
    /// </remarks>
    public string KeycloakId { get; private set; } = string.Empty;

    /// <summary>
    /// Indica se o usuário foi excluído logicamente do sistema.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Data e hora da exclusão lógica do usuário (UTC).
    /// </summary>
    /// <remarks>
    /// Será null se o usuário não foi excluído.
    /// </remarks>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Data e hora da última mudança de username (UTC).
    /// </summary>
    /// <remarks>
    /// Usado para implementar rate limiting nas mudanças de username.
    /// </remarks>
    public DateTime? LastUsernameChangeAt { get; private set; }

    /// <summary>
    /// Construtor privado para uso do Entity Framework.
    /// </summary>
    private User() { }

    /// <summary>
    /// Cria um novo usuário no sistema.
    /// </summary>
    /// <param name="username">Nome de usuário único</param>
    /// <param name="email">Endereço de email único</param>
    /// <param name="firstName">Primeiro nome</param>
    /// <param name="lastName">Sobrenome</param>
    /// <param name="keycloakId">ID do usuário no Keycloak</param>
    /// <remarks>
    /// Este construtor dispara automaticamente o evento UserRegisteredDomainEvent.
    /// </remarks>
    /// <exception cref="UserDomainException">Thrown when business rules are violated</exception>
    public User(Username username, Email email, string firstName, string lastName, string keycloakId)
        : base(UserId.New())
    {
        // Validações de regras de negócio específicas para criação
        ValidateUserCreation(keycloakId);

        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        KeycloakId = keycloakId;

        AddDomainEvent(new UserRegisteredDomainEvent(Id.Value, 1, email.Value, username.Value, firstName, lastName));
    }

    /// <summary>
    /// Atualiza as informações básicas do perfil do usuário (nome e sobrenome).
    /// </summary>
    /// <param name="firstName">Novo primeiro nome</param>
    /// <param name="lastName">Novo sobrenome</param>
    /// <remarks>
    /// ⚠️ OPERAÇÃO ESPECÍFICA: Este método atualiza APENAS o nome e sobrenome.
    /// 
    /// Para alterar outras informações, use os métodos específicos:
    /// - Para alterar email: Use ChangeEmail(newEmail)
    /// - Para alterar username: Use ChangeUsername(newUsername)
    /// 
    /// **Benefícios da separação:**
    /// - Validações específicas por tipo de dado
    /// - Melhor controle de regras de negócio
    /// - Logs e eventos mais granulares
    /// - Princípio da Responsabilidade Única
    /// 
    /// **Comportamento:**
    /// - Se os dados não mudaram, o método retorna sem fazer alterações
    /// - Quando alterações são feitas, dispara o evento UserProfileUpdatedDomainEvent
    /// - Aplica validações específicas para nome e sobrenome
    /// </remarks>
    /// <exception cref="UserDomainException">
    /// Lançada quando:
    /// - Usuário está deletado
    /// - Nome ou sobrenome são vazios
    /// - Nome ou sobrenome não atendem aos critérios de tamanho (2-100 caracteres)
    /// </exception>
    public void UpdateProfile(string firstName, string lastName)
    {
        ValidateProfileUpdate();

        if (FirstName == firstName && LastName == lastName)
            return;

        FirstName = firstName;
        LastName = lastName;
        MarkAsUpdated();

        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id.Value, 1, firstName, lastName));
    }

    /// <summary>
    /// Marca o usuário como excluído logicamente do sistema.
    /// </summary>
    /// <param name="dateTimeProvider">Provedor de data/hora para testabilidade</param>
    /// <remarks>
    /// Implementa exclusão lógica (soft delete) em vez de remoção física dos dados.
    /// Dispara o evento UserDeletedDomainEvent quando a exclusão é realizada.
    /// Se o usuário já estiver excluído, o método retorna sem fazer alterações.
    /// </remarks>
    public void MarkAsDeleted(IDateTimeProvider dateTimeProvider)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = dateTimeProvider.CurrentDate();
        MarkAsUpdated();

        AddDomainEvent(new UserDeletedDomainEvent(Id.Value, 1));
    }

    /// <summary>
    /// Retorna o nome completo do usuário.
    /// </summary>
    /// <returns>Nome completo formatado como "PrimeiroNome Sobrenome"</returns>
    /// <remarks>
    /// Remove espaços extras se um dos nomes estiver vazio.
    /// </remarks>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Valida regras de negócio para criação de usuário
    /// </summary>
    /// <param name="keycloakId">Identificador externo do Keycloak</param>
    /// <exception cref="UserDomainException">Lançada quando a validação falha</exception>
    private static void ValidateUserCreation(string keycloakId)
    {
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw UserDomainException.ForValidationError(nameof(keycloakId), keycloakId, "Keycloak ID is required for user creation");
    }

    /// <summary>
    /// Valida regras de negócio para atualizações de perfil
    /// </summary>
    /// <exception cref="UserDomainException">Lançada quando a validação falha</exception>
    private void ValidateProfileUpdate()
    {
        if (IsDeleted)
            throw UserDomainException.ForInvalidOperation("UpdateProfile", "user is deleted");
    }

    /// <summary>
    /// Altera o endereço de email do usuário
    /// </summary>
    /// <param name="newEmail">Novo endereço de email</param>
    /// <exception cref="UserDomainException">Lançada quando o usuário está deletado</exception>
    /// <remarks>
    /// Este método deve ser usado com cuidado, pois requer sincronização com o Keycloak.
    /// Considere implementar ações compensatórias se a atualização do Keycloak falhar.
    /// </remarks>
    public void ChangeEmail(string newEmail)
    {
        if (IsDeleted)
            throw UserDomainException.ForInvalidOperation("ChangeEmail", "user is deleted");

        if (Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
            return; // Nenhuma mudança necessária

        var oldEmail = Email;
        Email = newEmail;
        MarkAsUpdated();

        // Adiciona evento de domínio para sincronização com sistemas externos
        AddDomainEvent(new UserEmailChangedEvent(Id.Value, 1, oldEmail, newEmail));
    }

    /// <summary>
    /// Altera o nome de usuário (username)
    /// </summary>
    /// <param name="newUsername">Novo nome de usuário</param>
    /// <param name="dateTimeProvider">Provedor de data/hora para testabilidade</param>
    /// <exception cref="UserDomainException">Lançada quando o usuário está deletado</exception>
    /// <remarks>
    /// Este método deve ser usado com cuidado, pois requer sincronização com o Keycloak.
    /// Mudanças de username podem afetar a autenticação e devem ser validadas quanto à unicidade.
    /// </remarks>
    public void ChangeUsername(string newUsername, IDateTimeProvider dateTimeProvider)
    {
        if (IsDeleted)
            throw UserDomainException.ForInvalidOperation("ChangeUsername", "user is deleted");

        if (Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase))
            return; // Nenhuma mudança necessária

        var oldUsername = Username;
        Username = newUsername;
        LastUsernameChangeAt = dateTimeProvider.CurrentDate();
        MarkAsUpdated();

        // Adiciona evento de domínio para sincronização com sistemas externos
        AddDomainEvent(new UserUsernameChangedEvent(Id.Value, 1, oldUsername, newUsername));
    }

    /// <summary>
    /// Verifica se o usuário pode alterar o username baseado em rate limiting.
    /// </summary>
    /// <param name="dateTimeProvider">Provedor de data/hora para testabilidade</param>
    /// <param name="minimumDaysBetweenChanges">Número mínimo de dias entre mudanças de username</param>
    /// <returns>True se pode alterar, False se deve aguardar</returns>
    public bool CanChangeUsername(IDateTimeProvider dateTimeProvider, int minimumDaysBetweenChanges = 30)
    {
        if (LastUsernameChangeAt == null)
            return true;

        var daysSinceLastChange = (dateTimeProvider.CurrentDate() - LastUsernameChangeAt.Value).TotalDays;
        return daysSinceLastChange >= minimumDaysBetweenChanges;
    }
}