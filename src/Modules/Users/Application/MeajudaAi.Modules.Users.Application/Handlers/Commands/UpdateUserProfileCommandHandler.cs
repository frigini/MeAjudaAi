using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

public sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository
) : ICommandHandler<UpdateUserProfileCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> HandleAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            new UserId(command.UserId), cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure("User not found");

        user.UpdateProfile(command.FirstName, command.LastName);

        await userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserDto>.Success(user.ToDto());
    }
}