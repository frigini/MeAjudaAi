namespace MeAjudaAi.E2E.Tests.Integration.DTOs.Users;

/// <summary>
/// Modelo de requisição para atualizar perfil de usuário em testes E2E.
/// </summary>
public record UpdateUserProfileRequest
{
    /// <summary>
    /// Obtém ou inicializa o primeiro nome.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Obtém ou inicializa o último nome.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Obtém ou inicializa o endereço de email.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
