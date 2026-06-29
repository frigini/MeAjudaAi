using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de atualização do token de dispositivo de usuários.
/// </summary>
/// <param name="uow"></param>
/// <param name="usersCacheService"></param>
/// <param name="logger"></param>
public sealed class UpdateUserDeviceTokenCommandHandler(
    [FromKeyedServices(ModuleKeys.Users)] IUnitOfWork uow,
    IUsersCacheService usersCacheService,
    ILogger<UpdateUserDeviceTokenCommandHandler> logger
) : ICommandHandler<UpdateUserDeviceTokenCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateUserDeviceTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await uow.GetRepository<User, UserId>().TryFindAsync(
            new UserId(command.UserId), cancellationToken);

        if (user == null)
        {
            logger.LogWarning("User {UserId} not found for device token update.", command.UserId);
            return Result.Failure(Error.NotFound("Usuário não encontrado."));
        }

        user.UpdateDeviceToken(command.DeviceToken);
        await uow.SaveChangesAsync(cancellationToken);
        await usersCacheService.InvalidateUserAsync(command.UserId, user.Email.Value, cancellationToken);

        logger.LogInformation("Device token updated successfully for user {UserId}.", command.UserId);
        return Result.Success();
    }
}
