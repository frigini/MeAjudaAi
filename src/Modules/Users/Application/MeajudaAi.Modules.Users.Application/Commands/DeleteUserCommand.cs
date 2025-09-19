using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para exclusão lógica (soft delete) de um usuário.
/// </summary>
public sealed record DeleteUserCommand(Guid UserId) : Command<Result>;