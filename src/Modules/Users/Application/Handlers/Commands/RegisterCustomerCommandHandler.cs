using System.Text.RegularExpressions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Utilities;
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
    public const string TermsNotAcceptedError = "Você deve aceitar os termos de uso para se cadastrar.";
    public const string PrivacyPolicyNotAcceptedError = "Você deve aceitar a política de privacidade para se cadastrar.";
    public const string FailedToCompensateKeycloakUserMessage = "CRITICAL: Failed to compensate Keycloak user {UserId} after repository failure. Manual cleanup required.";
    public const string FailedToSaveRegistrationError = "Falha ao salvar o cadastro. Tente novamente mais tarde.";

    [GeneratedRegex(@"[^a-zA-Z0-9._\-]", RegexOptions.Compiled)]
    private static partial Regex SanitizationRegex();

    public async Task<Result<UserDto>> HandleAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.TermsAccepted)
        {
            return Result<UserDto>.Failure(Error.BadRequest(TermsNotAcceptedError));
        }

        if (!command.AcceptedPrivacyPolicy)
        {
            return Result<UserDto>.Failure(Error.BadRequest(PrivacyPolicyNotAcceptedError));
        }

        Email emailAsValueObject;
        Username validUsername;

        try
        {
            emailAsValueObject = new Email(command.Email);

            var emailSpan = emailAsValueObject.Value.AsSpan();
            var atIdx = emailSpan.IndexOf('@');
            var localSpan = atIdx >= 0 ? emailSpan[..atIdx] : emailSpan;
            var plusIdx = localSpan.IndexOf('+');
            var noTagSpan = plusIdx >= 0 ? localSpan[..plusIdx] : localSpan;
            var sanitizedLocalPart = SanitizationRegex().Replace(noTagSpan.ToString(), "");

            if (string.IsNullOrWhiteSpace(sanitizedLocalPart) || sanitizedLocalPart.Length < 3)
            {
                sanitizedLocalPart = $"usr{Guid.NewGuid().ToString("N").AsSpan(0, 5).ToString()}";
            }

            // UsernameMaxLength é 30 em ValidationConstants; deduz 1 para '_' e 6 para GUID => localPartMax = UsernameMaxLength - 7
            int maxLocalPartLength = ValidationConstants.UserLimits.UsernameMaxLength - 7;
            if (sanitizedLocalPart.Length > maxLocalPartLength)
            {
                sanitizedLocalPart = sanitizedLocalPart[..maxLocalPartLength];
            }

            var slug = $"{sanitizedLocalPart}_{Guid.NewGuid().ToString("N").AsSpan(0, 6).ToString()}";
            validUsername = new Username(slug);
        }
        catch (ArgumentException ex)
        {
            return Result<UserDto>.Failure(Error.BadRequest(ex.Message));
        }

        var rawEmailSpan = command.Email.AsSpan();
        var atSeparator = rawEmailSpan.IndexOf('@');
        var maskedEmail = atSeparator >= 0
            ? $"{new string('*', Math.Min(3, atSeparator))}@{rawEmailSpan[(atSeparator + 1)..].ToString()}"
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
        
        if (firstName.Length < ValidationConstants.UserLimits.FirstNameMinLength)
        {
            return Result<UserDto>.Failure(Error.BadRequest($"O primeiro nome deve ter pelo menos {ValidationConstants.UserLimits.FirstNameMinLength} caracteres."));
        }
        
        if (names.Length < 2 || string.IsNullOrWhiteSpace(names[1]))
        {
            return Result<UserDto>.Failure(Error.BadRequest($"O sobrenome é obrigatório e deve ter pelo menos {ValidationConstants.UserLimits.LastNameMinLength} caracteres."));
        }

        var lastName = names[1];
        if (lastName.Length < ValidationConstants.UserLimits.LastNameMinLength)
        {
            return Result<UserDto>.Failure(Error.BadRequest($"O sobrenome deve ter pelo menos {ValidationConstants.UserLimits.LastNameMinLength} caracteres."));
        }
        
        var userResult = await userDomainService.CreateUserAsync(
            validUsername,
            emailAsValueObject,
            firstName,
            lastName,
            command.Password,
            new[] { UserRoles.Customer }, // papel de cliente
            command.PhoneNumber,
            cancellationToken
        );

        if (userResult.IsFailure)
        {
            logger.LogWarning("Failed to register customer {Email}: {Error}", maskedEmail, userResult.Error);
            return Result<UserDto>.Failure(userResult.Error);
        }

        if (userResult.Value is null)
        {
            logger.LogCritical("User returned null from success result for {Email}", maskedEmail);
            return Result<UserDto>.Failure(Error.Internal("Falha crítica ao criar o usuário. Dados nulos retornados."));
        }

        var user = userResult.Value;

        try
        {
            await userRepository.AddAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                logger.LogWarning("RegisterCustomerCommand was canceled during repository persistence. Starting compensation.");
            }
            else
            {
                logger.LogError(ex, "Failed to persist customer {Email} ({Id}) to repository. Attempting Keycloak compensation.",
                    maskedEmail, user.Id);
            }

            // Verifica se o usuário realmente não foi salvo no repositório antes da compensação
            // Usamos CancellationToken.None para garantir que a compensação ocorra mesmo se o request original foi cancelado
            var persistenceCheck = await userRepository.GetByIdNoTrackingAsync(user.Id, CancellationToken.None);
            if (persistenceCheck == null)
            {
                // Compensação: desativar o usuário criado no Keycloak para evitar usuário órfão "fantasma" que pode logar mas não tem dados locais
                try
                {
                    var compensationResult = await userDomainService.DeactivateUserInKeycloakAsync(user.Id, CancellationToken.None);
                    if (compensationResult.IsFailure)
                    {
                        logger.LogError("Compensation failed for user {UserId}: {Error}", user.Id, compensationResult.Error);
                    }
                    else
                    {
                        logger.LogInformation("Keycloak user {UserId} deactivated successfully as compensation.", user.Id);
                    }
                }
                catch (Exception compensationEx)
                {
                    logger.LogCritical(compensationEx, FailedToCompensateKeycloakUserMessage, user.Id);
                }
            }
            else
            {
                logger.LogWarning("Repository write failure reported but user {UserId} was found in DB. Skipping Keycloak compensation.", user.Id);
            }

            if (ex is OperationCanceledException)
                throw;

            return Result<UserDto>.Failure(Error.Internal(FailedToSaveRegistrationError));
        }

        logger.LogInformation("Customer registered successfully: {Email} ({Id})", maskedEmail, user.Id);

        return Result<UserDto>.Success(user.ToDto());
    }
}
