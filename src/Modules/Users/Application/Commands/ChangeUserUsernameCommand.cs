using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para alteração do nome de usuário (username).
/// </summary>
/// <remarks>
/// Aplica validações de formato, unicidade e rate limiting (30 dias).
/// Administradores podem usar BypassRateLimit para contornar o limite de tempo.
/// </remarks>
/// <param name="UserId">Identificador único do usuário</param>
/// <param name="NewUsername">Novo nome de usuário</param>
/// <param name="UpdatedBy">Identificador de quem está fazendo a alteração</param>
/// <param name="BypassRateLimit">Permite bypasser limite de frequência (apenas admins)</param>
public sealed record ChangeUserUsernameCommand(
    Guid UserId,
    string NewUsername,
    string? UpdatedBy = null,
    bool BypassRateLimit = false
) : Command<Result<UserDto>>;