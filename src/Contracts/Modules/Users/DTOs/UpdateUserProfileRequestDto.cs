using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Users.DTOs;

/// <summary>
/// DTO para atualização de perfil de usuário via API.
/// </summary>
/// <remarks>
/// Para deixar Email ou PhoneNumber inalterados, envie null.
/// Strings vazias ou whitespace resultarão em erro de validação de domínio.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed record UpdateUserProfileRequestDto(
    string FirstName,
    string LastName,
    string? Email = null,
    string? PhoneNumber = null
);
