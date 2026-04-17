using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.Commands;

/// <summary>
/// Comando para exclusão lógica (soft delete) de um usuário.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DeleteUserCommand(Guid UserId) : Command<Result>;
