using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

public sealed class RegisterCustomerCommandHandler(
    IUserDomainService userDomainService,
    IUserRepository userRepository,
    ILogger<RegisterCustomerCommandHandler> logger
) : ICommandHandler<RegisterCustomerCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> HandleAsync(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        if (!command.TermsAccepted)
        {
            return Result<UserDto>.Failure(Error.BadRequest("Você deve aceitar os termos de uso para se cadastrar."));
        }

        // Validate uniqueness first
        var existingEmail = await userRepository.GetByEmailAsync(new Email(command.Email), cancellationToken);
        if (existingEmail is not null)
        {
            return Result<UserDto>.Failure(Error.Conflict("Este email já está em uso."));
        }

        // Create user with "customer" role
        // Provide "Customer" as FirstName and nothing as LastName if only Name is provided?
        // Or split Name? simpler to just pass Name as FirstName for now if we don't have split logic handy, 
        // but CreateUserCommand has FirstName and LastName.
        // Let's split simple name assume first and last
        var names = command.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = names.FirstOrDefault() ?? command.Name;
        var lastName = names.Length > 1 ? names[1] : "";

        // Use Email as Username for customers
        var username = new Username(command.Email);
        
        var userResult = await userDomainService.CreateUserAsync(
            username,
            new Email(command.Email),
            firstName,
            lastName,
            command.Password,
            new[] { "customer" }, // customer role
            command.PhoneNumber,
            cancellationToken
        );

        if (userResult.IsFailure)
        {
            logger.LogWarning("Failed to register customer {Email}: {Error}", command.Email, userResult.Error);
            return Result<UserDto>.Failure(userResult.Error);
        }

        await userRepository.AddAsync(userResult.Value, cancellationToken);
        
        logger.LogInformation("Customer registered successfully: {Email} ({Id})", command.Email, userResult.Value.Id);

        return Result<UserDto>.Success(userResult.Value.ToDto());
    }
}
