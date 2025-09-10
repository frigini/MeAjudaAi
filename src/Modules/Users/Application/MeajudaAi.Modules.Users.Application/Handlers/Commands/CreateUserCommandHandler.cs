using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

public sealed class CreateUserCommandHandler(
    IUserDomainService userDomainService,
    IUserRepository userRepository
) : ICommandHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var existingByEmail = await userRepository.GetByEmailAsync(
                new Email(command.Email), cancellationToken);
            if (existingByEmail != null)
                return Result<UserDto>.Failure("User with this email already exists");

            var existingByUsername = await userRepository.GetByUsernameAsync(
                new Username(command.Username), cancellationToken);
            if (existingByUsername != null)
                return Result<UserDto>.Failure("Username already taken");

            // Create user through domain service
            var userResult = await userDomainService.CreateUserAsync(
                new Username(command.Username),
                new Email(command.Email),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                cancellationToken);

            if (userResult.IsFailure)
                return Result<UserDto>.Failure(userResult.Error);

            // Save to repository
            await userRepository.AddAsync(userResult.Value, cancellationToken);

            return Result<UserDto>.Success(userResult.Value.ToDto());
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to create user: {ex.Message}");
        }
    }
}