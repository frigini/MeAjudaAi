using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para atualização do perfil básico do usuário (nome e sobrenome).
/// Para alterações de email ou username, use comandos específicos.
/// </summary>
public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? UpdatedBy = null
) : Command<Result<UserDto>>;
