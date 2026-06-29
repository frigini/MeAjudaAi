using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para exclusão lógica (soft delete) de um usuário.
/// </summary>
public sealed record DeleteUserCommand(Guid UserId) : Command<Result>;