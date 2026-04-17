using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para criação de um novo usuário no sistema.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record CreateUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    IEnumerable<string> Roles,
    string? PhoneNumber = null
) : Command<Result<UserDto>>;
