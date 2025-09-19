using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.Exceptions;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Common;

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
        // Business rule validations
        ValidateUserCreation(firstName, lastName, keycloakId);

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
        ValidateProfileUpdate(firstName, lastName);
        
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
    /// <remarks>
    /// Implementa exclusão lógica (soft delete) em vez de remoção física dos dados.
    /// Dispara o evento UserDeletedDomainEvent quando a exclusão é realizada.
    /// Se o usuário já estiver excluído, o método retorna sem fazer alterações.
    /// </remarks>
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
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
    /// Validates business rules for user creation
    /// </summary>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="keycloakId">Keycloak external identifier</param>
    /// <exception cref="UserDomainException">Thrown when validation fails</exception>
    private static void ValidateUserCreation(string firstName, string lastName, string keycloakId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw UserDomainException.ForValidationError(nameof(firstName), firstName, "First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw UserDomainException.ForValidationError(nameof(lastName), lastName, "Last name cannot be empty");

        if (string.IsNullOrWhiteSpace(keycloakId))
            throw UserDomainException.ForValidationError(nameof(keycloakId), keycloakId, "Keycloak ID is required for user creation");

        if (firstName.Length < 2 || firstName.Length > 100)
            throw UserDomainException.ForValidationError(nameof(firstName), firstName, "First name must be between 2 and 100 characters");

        if (lastName.Length < 2 || lastName.Length > 100)
            throw UserDomainException.ForValidationError(nameof(lastName), lastName, "Last name must be between 2 and 100 characters");
    }

    /// <summary>
    /// Validates business rules for profile updates
    /// </summary>
    /// <param name="firstName">New first name</param>
    /// <param name="lastName">New last name</param>
    /// <exception cref="UserDomainException">Thrown when validation fails</exception>
    private void ValidateProfileUpdate(string firstName, string lastName)
    {
        if (IsDeleted)
            throw UserDomainException.ForInvalidOperation("UpdateProfile", "user is deleted");

        if (string.IsNullOrWhiteSpace(firstName))
            throw UserDomainException.ForValidationError(nameof(firstName), firstName, "First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw UserDomainException.ForValidationError(nameof(lastName), lastName, "Last name cannot be empty");

        if (firstName.Length < 2 || firstName.Length > 100)
            throw UserDomainException.ForValidationError(nameof(firstName), firstName, "First name must be between 2 and 100 characters");

        if (lastName.Length < 2 || lastName.Length > 100)
            throw UserDomainException.ForValidationError(nameof(lastName), lastName, "Last name must be between 2 and 100 characters");
    }

    /// <summary>
    /// Changes the user's email address
    /// </summary>
    /// <param name="newEmail">New email address</param>
    /// <exception cref="UserDomainException">Thrown when validation fails</exception>
    /// <remarks>
    /// This method should be used carefully as it requires synchronization with Keycloak.
    /// Consider implementing compensating actions if Keycloak update fails.
    /// </remarks>
    public void ChangeEmail(string newEmail)
    {
        if (IsDeleted)
            throw UserDomainException.ForInvalidOperation("ChangeEmail", "user is deleted");

        if (string.IsNullOrWhiteSpace(newEmail))
            throw UserDomainException.ForValidationError("email", newEmail, "Email cannot be empty");

        if (newEmail.Length > 255)
            throw UserDomainException.ForValidationError("email", newEmail, "Email cannot exceed 255 characters");

        if (!IsValidEmail(newEmail))
            throw UserDomainException.ForInvalidFormat("email", newEmail, "valid email format (example@domain.com)");

        if (Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
            return; // No change needed

        var oldEmail = Email;
        Email = newEmail;
        
        // Add domain event for external system synchronization
        AddDomainEvent(new UserEmailChangedEvent(Id.Value, 1, oldEmail, newEmail));
    }

    /// <summary>
    /// Changes the user's username
    /// </summary>
    /// <param name="newUsername">New username</param>
    /// <exception cref="UserDomainException">Thrown when validation fails</exception>
    /// <remarks>
    /// This method should be used carefully as it requires synchronization with Keycloak.
    /// Username changes may affect authentication and should be validated for uniqueness.
    /// </remarks>
    public void ChangeUsername(string newUsername)
    {
        if (IsDeleted)
            throw UserDomainException.ForInvalidOperation("ChangeUsername", "user is deleted");

        if (string.IsNullOrWhiteSpace(newUsername))
            throw UserDomainException.ForValidationError("username", newUsername, "Username cannot be empty");

        if (newUsername.Length < 3 || newUsername.Length > 50)
            throw UserDomainException.ForValidationError("username", newUsername, "Username must be between 3 and 50 characters");

        if (!IsValidUsername(newUsername))
            throw UserDomainException.ForInvalidFormat("username", newUsername, "letters, numbers, dots, hyphens and underscores only");

        if (Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase))
            return; // No change needed

        var oldUsername = Username;
        Username = newUsername;
        LastUsernameChangeAt = DateTime.UtcNow;
        
        // Add domain event for external system synchronization
        AddDomainEvent(new UserUsernameChangedEvent(Id.Value, 1, oldUsername, newUsername));
    }

    /// <summary>
    /// Verifica se o usuário pode alterar o username baseado em rate limiting.
    /// </summary>
    /// <param name="minimumDaysBetweenChanges">Número mínimo de dias entre mudanças de username</param>
    /// <returns>True se pode alterar, False se deve aguardar</returns>
    public bool CanChangeUsername(int minimumDaysBetweenChanges = 30)
    {
        if (LastUsernameChangeAt == null)
            return true;
            
        var daysSinceLastChange = (DateTime.UtcNow - LastUsernameChangeAt.Value).TotalDays;
        return daysSinceLastChange >= minimumDaysBetweenChanges;
    }

    /// <summary>
    /// Validates email format using basic regex pattern
    /// </summary>
    /// <param name="email">Email to validate</param>
    /// <returns>True if email format is valid</returns>
    private static bool IsValidEmail(string email)
    {
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern);
    }

    /// <summary>
    /// Validates username format (alphanumeric, dots, hyphens, underscores)
    /// </summary>
    /// <param name="username">Username to validate</param>
    /// <returns>True if username format is valid</returns>
    private static bool IsValidUsername(string username)
    {
        var usernamePattern = @"^[a-zA-Z0-9._-]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(username, usernamePattern);
    }
}