using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUserDomainService userDomainService
) : ICommandHandler<DeleteUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            new UserId(command.UserId), cancellationToken);

        if (user == null)
            return Result.Failure("User not found");

        try
        {
            // Deactivate in Keycloak first
            var syncResult = await userDomainService.SyncUserWithKeycloakAsync(
                user.Id, cancellationToken);

            if (syncResult.IsFailure)
                return syncResult;

            // Soft delete in local database
            // Note: You might want to add a soft delete method to your User entity
            // For now, we could mark as deleted or just remove from repo

            // Option 1: If you have soft delete in User entity
            // user.MarkAsDeleted();
            // await userRepository.UpdateAsync(user, cancellationToken);

            // Option 2: Hard delete (not recommended for production)
            await userRepository.DeleteAsync(user.Id, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }
}