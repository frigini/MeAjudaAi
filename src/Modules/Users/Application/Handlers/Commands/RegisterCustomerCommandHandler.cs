using System.Text.RegularExpressions;
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
    public async Task<Result<UserDto>> HandleAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.TermsAccepted)
        {
            return Result<UserDto>.Failure(Error.BadRequest("Você deve aceitar os termos de uso para se cadastrar."));
        }

        Email emailAsValueObject;
        Username validUsername;

        try
        {
            emailAsValueObject = new Email(command.Email);
            
            var fullLocalPart = emailAsValueObject.Value.Split('@')[0];
            var noTagLocalPart = fullLocalPart.Split('+')[0];
            var sanitizedLocalPart = Regex.Replace(noTagLocalPart, @"[^a-zA-Z0-9._\-]", "");
            
            if (string.IsNullOrWhiteSpace(sanitizedLocalPart) || sanitizedLocalPart.Length < 3)
            {
                sanitizedLocalPart = $"user{Guid.NewGuid().ToString("N").Substring(0, 5)}";
            }
            
            var slug = $"{sanitizedLocalPart}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            validUsername = new Username(slug);
        }
        catch (ArgumentException ex)
        {
            return Result<UserDto>.Failure(Error.BadRequest(ex.Message));
        }

        var emailParts = command.Email.Split('@');
        var maskedEmail = emailParts.Length == 2 
            ? $"{new string('*', Math.Min(3, emailParts[0].Length))}@{emailParts[1]}" 
            : "***@***";

        // Valida unicidade primeiro
        var existingEmail = await userRepository.GetByEmailAsync(emailAsValueObject, cancellationToken);
        if (existingEmail is not null)
        {
            return Result<UserDto>.Failure(Error.Conflict("Este email já está em uso."));
        }

        // Cria usuário com papel de "cliente"
        var names = command.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = names.FirstOrDefault() ?? command.Name;
        var lastName = names.Length > 1 ? names[1] : "";
        
        var userResult = await userDomainService.CreateUserAsync(
            validUsername,
            emailAsValueObject,
            firstName,
            lastName,
            command.Password,
            new[] { "customer" }, // papel de cliente
            command.PhoneNumber,
            cancellationToken
        );

        if (userResult.IsFailure)
        {
            logger.LogWarning("Failed to register customer {Email}: {Error}", maskedEmail, userResult.Error);
            return Result<UserDto>.Failure(userResult.Error);
        }

        await userRepository.AddAsync(userResult.Value, cancellationToken);
        
        logger.LogInformation("Customer registered successfully: {Email} ({Id})", maskedEmail, userResult.Value.Id);

        return Result<UserDto>.Success(userResult.Value.ToDto());
    }
}
