using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para criação de um novo usuário no sistema.
/// </summary>
public sealed record CreateUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    IEnumerable<string> Roles
) : Command<Result<UserDto>>;