using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Commands;

public sealed record DeleteUserCommand(Guid UserId) : Command<Result>;