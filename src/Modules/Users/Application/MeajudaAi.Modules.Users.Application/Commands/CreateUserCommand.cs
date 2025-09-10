using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Commands;

public sealed record CreateUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    IEnumerable<string> Roles
) : Command<Result<UserDto>>;