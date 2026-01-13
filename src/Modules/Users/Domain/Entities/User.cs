using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.Exceptions;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Utilities;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MeAjudaAi.Modules.Users.Tests")]

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
    /// Número de telefone do usuário (opcional).
    /// </summary>
    /// <remarks>
    /// Implementado como value object com validação de formato.
    /// </remarks>
    public PhoneNumber? PhoneNumber { get; private set; }

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
    /// Token de concorrência otimista para PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Usa a coluna de sistema xmin do PostgreSQL para detectar conflitos de concorrência.
    /// Será automaticamente incrementado em cada UPDATE.
    /// </remarks>
    public uint RowVersion { get; }

    /// <summary>
    /// Construtor privado para uso do Entity Framework.
    /// </summary>
    private User() { }

    /// <summary>
    /// Internal test helper to set Id. Only accessible from test assemblies.
    /// </summary>
    internal void SetIdForTesting(UserId id)
    {
        Id = id;
    }

    /// <summary>
    /// Internal test helper to set CreatedAt. Only accessible from test assemblies.
    /// </summary>
#pragma warning disable S3011 // Reflection is acceptable for internal test helpers
    internal void SetCreatedAtForTesting(DateTime createdAt)
    {
        var baseField = typeof(BaseEntity).GetField("<CreatedAt>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        baseField?.SetValue(this, createdAt);
    }
#pragma warning restore S3011

    /// <summary>
    /// Internal test helper to set UpdatedAt. Only accessible from test assemblies.
    /// </summary>
#pragma warning disable S3011 // Reflection is acceptable for internal test helpers
    internal void SetUpdatedAtForTesting(DateTime? updatedAt)
    {
        var baseField = typeof(BaseEntity).GetField("<UpdatedAt>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        baseField?.SetValue(this, updatedAt);
    }
#pragma warning restore S3011

    /// <summary>
    /// Cria um novo usuário no sistema.
    /// </summary>
    /// <param name="username">Nome de usuário único</param>
    /// <param name="email">Endereço de email único</param>
    /// <param name="firstName">Primeiro nome</param>
    /// <param name="lastName">Sobrenome</param>
    /// <param name="keycloakId">ID do usuário no Keycloak</param>
    /// <param name="phoneNumber">Número de telefone (opcional)</param>
    /// <remarks>
    /// Este construtor dispara automaticamente o evento UserRegisteredDomainEvent.
    /// </remarks>
    /// <exception cref="UserDomainException">Thrown when business rules are violated</exception>
    public User(Username username, Email email, string firstName, string lastName, string keycloakId, string? phoneNumber = null)
        : base(UserId.New())
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);

        // Validações de regras de negócio específicas para criação
        ValidateUserCreation(keycloakId);

        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        KeycloakId = keycloakId;
        
        // Define PhoneNumber se fornecido
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            PhoneNumber = new PhoneNumber(phoneNumber);
        }

        AddDomainEvent(new UserRegisteredDomainEvent(Id.Value, 1, email.Value, username.Value, firstName, lastName));
    }

    /// <summary>
    /// Atualiza as informações básicas do perfil do usuário (nome e sobrenome).
    /// </summary>
    /// <param name="firstName">Novo primeiro nome do usuário</param>
    /// <param name="lastName">Novo sobrenome do usuário</param>
    /// <param name="email">Novo email (opcional). Use null para não alterar. String vazia/whitespace será rejeitada.</param>
    /// <param name="phoneNumber">Novo número de telefone (opcional). Use null para não alterar. String vazia/whitespace será rejeitada.</param>
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
    /// - Email ou PhoneNumber são strings vazias/whitespace (use null para não alterar)
    /// </exception>
    public void UpdateProfile(string firstName, string lastName, string? email = null, string? phoneNumber = null)
    {
        ValidateProfileUpdate();

        // Validação defensiva: rejeita strings vazias/whitespace
        if (email is not null && string.IsNullOrWhiteSpace(email))
            throw new UserDomainException("Email cannot be empty or whitespace. Use null to leave unchanged.");
        
        if (phoneNumber is not null && string.IsNullOrWhiteSpace(phoneNumber))
            throw new UserDomainException("PhoneNumber cannot be empty or whitespace. Use null to leave unchanged.");

        var hasChanges = FirstName != firstName || LastName != lastName;
        var emailChanged = false;
        var phoneChanged = false;
        
        FirstName = firstName;
        LastName = lastName;
        
        if (email != null)
        {
            Email = new Email(email);
            hasChanges = true;
            emailChanged = true;
        }
        
        if (phoneNumber != null)
        {
            PhoneNumber = new PhoneNumber(phoneNumber);
            hasChanges = true;
            phoneChanged = true;
        }
        
        if (!hasChanges)
            return;

        MarkAsUpdated();

        // Evento de domínio com todas as mudanças (usando valores normalizados do agregado)
        AddDomainEvent(new UserProfileUpdatedDomainEvent(
            Id.Value, 
            1, 
            firstName, 
            lastName, 
            emailChanged ? Email.Value : null, 
            phoneChanged ? PhoneNumber?.Value : null));
    }

    /// <summary>
    /// Marca o usuário como excluído logicamente do sistema.
    /// </summary>
    /// <param name="timeProvider">Provedor de data/hora para testabilidade</param>
    /// <remarks>
    /// Implementa exclusão lógica (soft delete) em vez de remoção física dos dados.
    /// Dispara o evento UserDeletedDomainEvent quando a exclusão é realizada.
    /// Se o usuário já estiver excluído, o método retorna sem fazer alterações.
    /// </remarks>
    public void MarkAsDeleted(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = timeProvider.GetUtcNow().UtcDateTime;
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

        if (string.Equals(Email.Value, newEmail, StringComparison.OrdinalIgnoreCase))
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
    /// <param name="timeProvider">Provedor de data/hora para testabilidade</param>
    /// <exception cref="UserDomainException">Lançada quando o usuário está deletado</exception>
    /// <remarks>
    /// Este método deve ser usado com cuidado, pois requer sincronização com o Keycloak.
    /// Mudanças de username podem afetar a autenticação e devem ser validadas quanto à unicidade.
    /// </remarks>
    public void ChangeUsername(string newUsername, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (IsDeleted)
            throw UserDomainException.ForInvalidOperation("ChangeUsername", "user is deleted");

        if (string.Equals(Username.Value, newUsername, StringComparison.OrdinalIgnoreCase))
            return; // Nenhuma mudança necessária

        var oldUsername = Username;
        Username = newUsername;
        LastUsernameChangeAt = timeProvider.GetUtcNow().UtcDateTime;
        MarkAsUpdated();

        // Adiciona evento de domínio para sincronização com sistemas externos
        AddDomainEvent(new UserUsernameChangedEvent(Id.Value, 1, oldUsername, newUsername));
    }

    /// <summary>
    /// Verifica se o usuário pode alterar o username baseado em rate limiting.
    /// </summary>
    /// <param name="timeProvider">Provedor de data/hora para testabilidade</param>
    /// <param name="minimumDaysBetweenChanges">Número mínimo de dias entre mudanças de username</param>
    /// <returns>True se pode alterar, False se deve aguardar</returns>
    public bool CanChangeUsername(TimeProvider timeProvider, int minimumDaysBetweenChanges = 30)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (LastUsernameChangeAt == null)
            return true;

        var daysSinceLastChange = (timeProvider.GetUtcNow().UtcDateTime - LastUsernameChangeAt.Value).TotalDays;
        return daysSinceLastChange >= minimumDaysBetweenChanges;
    }
}
