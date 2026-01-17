using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para atualização do perfil básico do usuário (nome, sobrenome, email, phone).
/// Para alterações de username, use comandos específicos.
/// </summary>
public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Email = null,
    string? PhoneNumber = null,
    string? UpdatedBy = null
) : Command<Result<UserDto>>;
