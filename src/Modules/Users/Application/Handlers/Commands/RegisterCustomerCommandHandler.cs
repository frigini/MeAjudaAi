using System.Text.RegularExpressions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

public sealed partial class RegisterCustomerCommandHandler(
    IUserDomainService userDomainService,
    IUserRepository userRepository,
    ILogger<RegisterCustomerCommandHandler> logger
) : ICommandHandler<RegisterCustomerCommand, Result<UserDto>>
{
    [GeneratedRegex(@"[^a-zA-Z0-9._\-]")]
    private static partial Regex SanitizationRegex();

    public async Task<Result<UserDto>> HandleAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.TermsAccepted)
        {
            return Result<UserDto>.Failure(Error.BadRequest("Você deve aceitar os termos de uso para se cadastrar."));
        }

        if (!command.AcceptedPrivacyPolicy)
        {
            return Result<UserDto>.Failure(Error.BadRequest("Você deve aceitar a política de privacidade para se cadastrar."));
        }

        Email emailAsValueObject;
        Username validUsername;

        try
        {
            emailAsValueObject = new Email(command.Email);
            
            var fullLocalPart = emailAsValueObject.Value.Split('@')[0];
            var noTagLocalPart = fullLocalPart.Split('+')[0];
            var sanitizedLocalPart = SanitizationRegex().Replace(noTagLocalPart, "");
            
            if (string.IsNullOrWhiteSpace(sanitizedLocalPart) || sanitizedLocalPart.Length < 3)
            {
                sanitizedLocalPart = $"usr{Guid.NewGuid().ToString("N").Substring(0, 5)}";
            }
            
            // UsernameMaxLength é 30 em ValidationConstants; deduz 1 para '_' e 6 para GUID => localPartMax = UsernameMaxLength - 7
            int maxLocalPartLength = ValidationConstants.UserLimits.UsernameMaxLength - 7;
            if (sanitizedLocalPart.Length > maxLocalPartLength)
            {
                sanitizedLocalPart = sanitizedLocalPart.Substring(0, maxLocalPartLength);
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
        var lastName = names.Length > 1 && !string.IsNullOrWhiteSpace(names[1]) ? names[1] : ".";
        
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

        try
        {
            await userRepository.AddAsync(userResult.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist customer {Email} ({Id}) to repository. Attempting Keycloak compensation.",
                maskedEmail, userResult.Value.Id);

            // Compensação: desativar o usuário criado no Keycloak para evitar órfão
            try
            {
                await userDomainService.SyncUserWithKeycloakAsync(userResult.Value.Id, cancellationToken);
            }
            catch (Exception compensationEx)
            {
                logger.LogCritical(compensationEx,
                    "CRITICAL: Failed to compensate Keycloak user {UserId} after repository failure. Manual cleanup required.",
                    userResult.Value.Id);
            }

            return Result<UserDto>.Failure(Error.Internal("Falha ao salvar o cadastro. Tente novamente mais tarde."));
        }

        logger.LogInformation("Customer registered successfully: {Email} ({Id})", maskedEmail, userResult.Value.Id);

        return Result<UserDto>.Success(userResult.Value.ToDto());
    }
}
