using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de registro de clientes.
/// </summary>
/// <param name="userDomainService"></param>
/// <param name="uow"></param>
/// <param name="userQueries"></param>
/// <param name="logger"></param>
public sealed partial class RegisterCustomerCommandHandler(
    IUserDomainService userDomainService,
    [FromKeyedServices(ModuleKeys.Users)] IUnitOfWork uow,
    IUserQueries userQueries,
    ILogger<RegisterCustomerCommandHandler> logger,
    IStringLocalizer<Strings> localizer
) : ICommandHandler<RegisterCustomerCommand, Result<UserDto>>
{
    public const string TermsNotAcceptedError = "Você deve aceitar os termos de uso para se cadastrar.";
    public const string PrivacyPolicyNotAcceptedError = "Você deve aceitar a política de privacidade para se cadastrar.";
    public const string FailedToCompensateKeycloakUserMessage = "CRITICAL: Failed to compensate Keycloak user {UserId} after repository failure. Manual cleanup required.";
    public const string FailedToSaveRegistrationError = "Falha ao salvar o cadastro. Tente novamente mais tarde.";

    private readonly IStringLocalizer<Strings> _localizer = localizer;

    [GeneratedRegex(@"[^a-zA-Z0-9._\-]", RegexOptions.Compiled)]
    private static partial Regex SanitizationRegex();

    public async Task<Result<UserDto>> HandleAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.TermsAccepted)
        {
            return Result<UserDto>.Failure(Error.BadRequest(_localizer["TermsAcceptanceRequired"]));
        }

        if (!command.AcceptedPrivacyPolicy)
        {
            return Result<UserDto>.Failure(Error.BadRequest(_localizer["PrivacyPolicyAcceptanceRequired"]));
        }

        Email emailAsValueObject;
        Username validUsername;

        try
        {
            emailAsValueObject = new Email(command.Email);
            
            var emailValueSpan = emailAsValueObject.Value.AsSpan();
            var atIndex = emailValueSpan.IndexOf('@');
            var localPartSpan = atIndex >= 0 ? emailValueSpan[..atIndex] : emailValueSpan;
            
            var plusIndex = localPartSpan.IndexOf('+');
            if (plusIndex >= 0)
            {
                localPartSpan = localPartSpan[..plusIndex];
            }
            
            var sanitizedLocalPart = SanitizationRegex().Replace(localPartSpan.ToString(), "");
            
            if (string.IsNullOrWhiteSpace(sanitizedLocalPart) || sanitizedLocalPart.Length < 3)
            {
                sanitizedLocalPart = $"usr{Guid.NewGuid().ToString("N")[..5]}";
            }
            
            // UsernameMaxLength é 30 em ValidationConstants; deduz 1 para '_' e 6 para GUID => localPartMax = UsernameMaxLength - 7
            int maxLocalPartLength = ValidationConstants.UserLimits.UsernameMaxLength - 7;
            if (sanitizedLocalPart.Length > maxLocalPartLength)
            {
                sanitizedLocalPart = sanitizedLocalPart[..maxLocalPartLength];
            }
            
            var slug = $"{sanitizedLocalPart}_{Guid.NewGuid().ToString("N")[..6]}";
            validUsername = new Username(slug);
        }
        catch (ArgumentException ex)
        {
            return Result<UserDto>.Failure(Error.BadRequest(ex.Message));
        }

        var maskedEmail = PiiMaskingHelper.MaskEmail(command.Email);

        // Valida unicidade primeiro
        var existingEmail = await userQueries.GetByEmailAsync(emailAsValueObject, cancellationToken);
        if (existingEmail is not null)
        {
            return Result<UserDto>.Failure(Error.Conflict(_localizer["EmailAlreadyExists"]));
        }

        // Cria usuário com papel de "cliente"
        var trimmedName = command.Name.Trim();
        var parts = trimmedName.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        var firstName = parts.Length > 0 ? parts[0] : string.Empty;
        var lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
        
        if (firstName.Length < ValidationConstants.UserLimits.FirstNameMinLength)
        {
            return Result<UserDto>.Failure(Error.BadRequest(_localizer["FirstNameMinLength", ValidationConstants.UserLimits.FirstNameMinLength]));
        }
        
        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result<UserDto>.Failure(Error.BadRequest(_localizer["LastNameRequired", ValidationConstants.UserLimits.LastNameMinLength]));
        }
        if (lastName.Length < ValidationConstants.UserLimits.LastNameMinLength)
        {
            return Result<UserDto>.Failure(Error.BadRequest(_localizer["LastNameMinLength", ValidationConstants.UserLimits.LastNameMinLength]));
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
            return Result<UserDto>.Failure(Error.Internal(_localizer["UserCreateCriticalError"]));
        }

        var user = userResult.Value;

        try
        {
            uow.GetRepository<Domain.Entities.User, UserId>().Add(user);
            await uow.SaveChangesAsync(cancellationToken);
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
            var persistenceCheck = await userQueries.GetByIdAsync(user.Id, CancellationToken.None);
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

            return Result<UserDto>.Failure(Error.Internal(_localizer["RegistrationSaveError"]));
        }

        logger.LogInformation("Customer registered successfully: {Email} ({Id})", maskedEmail, user.Id);

        return Result<UserDto>.Success(user.ToDto());
    }
}
