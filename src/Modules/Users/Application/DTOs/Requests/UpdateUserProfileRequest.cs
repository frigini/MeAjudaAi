using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

/// <summary>
/// Requisição para atualização de perfil de usuário.
/// </summary>
/// <remarks>
/// Para deixar Email ou PhoneNumber inalterados, envie null.
/// Strings vazias ou whitespace resultarão em erro de validação de domínio.
/// </remarks>
public record UpdateUserProfileRequest
{
    /// <summary>
    /// Primeiro nome do usuário (obrigatório).
    /// </summary>
    public string FirstName { get; init; } = string.Empty;
    
    /// <summary>
    /// Sobrenome do usuário (obrigatório).
    /// </summary>
    public string LastName { get; init; } = string.Empty;
    
    /// <summary>
    /// Email do usuário. Envie null para não alterar, string válida para atualizar.
    /// Strings vazias ou whitespace são rejeitadas.
    /// </summary>
    public string? Email { get; init; }
    
    /// <summary>
    /// Número de telefone do usuário. Envie null para não alterar, string válida para atualizar.
    /// Strings vazias ou whitespace são rejeitadas.
    /// </summary>
    public string? PhoneNumber { get; init; }
}
