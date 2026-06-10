using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Users.Application.Commands;

public sealed record UpdateUserDeviceTokenCommand(Guid UserId, string DeviceToken, Guid CorrelationId) : ICommand<Result>;
