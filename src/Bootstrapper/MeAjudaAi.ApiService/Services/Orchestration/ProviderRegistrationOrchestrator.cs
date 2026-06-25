using MeAjudaAi.ApiService.Services.Orchestration.Interfaces;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;
using System.Security.Cryptography;

namespace MeAjudaAi.ApiService.Services.Orchestration;

/// <summary>
/// Orquestra o registro de provedores, coordenando a criação de usuários e provedores com compensação em caso de falha.
/// </summary>
public sealed class ProviderRegistrationOrchestrator(
    ICommandDispatcher commandDispatcher,
    ILogger<ProviderRegistrationOrchestrator> logger) : IProviderRegistrationOrchestrator
{
    public async Task<Result<ProviderDto>> RegisterProviderAsync(
        RegisterProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (!request.AcceptedTerms || !request.AcceptedPrivacyPolicy)
            return Result<ProviderDto>.Failure(
                new Error("Você deve aceitar os Termos de Uso e a Política de Privacidade para se cadastrar.", 400));

        var phone = SanitizePhone(request.PhoneNumber ?? string.Empty);
        var username = GenerateUsername(phone);
        var (firstName, lastName) = SplitName(request.Name ?? string.Empty);
        var password = GenerateTemporaryPassword();

        var createUserResult = await CreateUserAsync(
            username,
            request.Email ?? string.Empty,
            firstName,
            lastName,
            password,
            request.PhoneNumber ?? string.Empty,
            cancellationToken
        );

        if (createUserResult.IsFailure)
        {
            var errorMessage = createUserResult.Error?.Message ?? "Unknown error";
            logger.LogError("Failed to create Keycloak user for provider registration. Error: {Error}", errorMessage);
            return Result<ProviderDto>.Failure(
                new Error("Ocorreu um erro ao registrar o usuário.", 400));
        }

        var userDto = createUserResult.Value;
        if (userDto is null)
        {
            logger.LogError("User creation returned success but value is null");
            return Result<ProviderDto>.Failure(
                new Error("Ocorreu um erro interno ao processar o cadastro.", 500));
        }

        var providerResult = await CreateProviderAsync(
            userDto.Id,
            request.Name ?? string.Empty,
            request.Type,
            request.Email ?? string.Empty,
            request.PhoneNumber ?? string.Empty,
            cancellationToken
        );

        if (providerResult.IsFailure)
        {
            await CompensateUserCreationAsync(userDto.Id, cancellationToken);
            return Result<ProviderDto>.Failure(
                new Error("Ocorreu um erro ao registrar o provedor.", 400));
        }

        var providerDto = providerResult.Value;
        if (providerDto is null)
        {
            logger.LogError("Provider creation returned success but value is null");
            return Result<ProviderDto>.Failure(
                new Error("Ocorreu um erro interno ao processar o cadastro.", 500));
        }

        return Result<ProviderDto>.Success(providerDto);
    }

    private async Task<Result<UserDto>> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string password,
        string phoneNumber,
        CancellationToken cancellationToken)
    {
        var createUserCommand = new CreateUserCommand(
            Username: username,
            Email: email,
            FirstName: firstName,
            LastName: lastName,
            Password: password,
            Roles: [TierToRoleString(EProviderTier.Standard)],
            PhoneNumber: phoneNumber);

        return await commandDispatcher.SendAsync<CreateUserCommand, Result<UserDto>>(
            createUserCommand, cancellationToken);
    }

    private async Task<Result<ProviderDto>> CreateProviderAsync(
        Guid userId,
        string name,
        EProviderType type,
        string email,
        string phoneNumber,
        CancellationToken cancellationToken)
    {
        var createProviderCommand = new CreateProviderCommand(
            UserId: userId,
            Name: name,
            Type: type,
            BusinessProfile: new BusinessProfileDto(
                LegalName: name,
                FantasyName: null,
                Description: null,
                ContactInfo: new ContactInfoDto(
                    Email: email,
                    PhoneNumber: phoneNumber,
                    Website: null),
                PrimaryAddress: new AddressDto(
                    Street: string.Empty,
                    Number: string.Empty,
                    Complement: null,
                    Neighborhood: string.Empty,
                    City: string.Empty,
                    State: string.Empty,
                    ZipCode: string.Empty,
                    Country: "BR")));

        return await commandDispatcher.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
            createProviderCommand, cancellationToken);
    }

    private async Task CompensateUserCreationAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var deleteUserCommand = new DeleteUserCommand(userId);
            var deleteResult = await commandDispatcher.SendAsync<DeleteUserCommand, MeAjudaAi.Contracts.Functional.Result>(deleteUserCommand, cts.Token);

            if (deleteResult.IsFailure)
            {
                var errorMessage = deleteResult.Error?.Message ?? "Unknown error";
                logger.LogError(
                    "Compensation failed: Could not delete orphaned user {UserId}. Error: {Error}",
                    userId,
                    errorMessage);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Compensation cancelled while attempting to delete orphaned user {UserId}.", userId);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Compensation failed (timeout): Could not delete orphaned user {UserId}.", userId);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Compensation failed (invalid operation): Could not delete orphaned user {UserId}.", userId);
        }
    }

    private static string SanitizePhone(string? phoneNumber)
    {
        return System.Text.RegularExpressions.Regex.Replace(phoneNumber ?? "", @"\D", "");
    }

    private static string GenerateUsername(string phone)
    {
        return string.IsNullOrEmpty(phone)
            ? $"provider_{Guid.NewGuid():N}"
            : $"provider_{phone}";
    }

    private static (string FirstName, string LastName) SplitName(string? name)
    {
        var safeName = name ?? string.Empty;
        var nameParts = safeName.Trim().Split(' ', 2);
        var firstName = nameParts.Length > 0 && !string.IsNullOrEmpty(nameParts[0]) ? nameParts[0] : string.Empty;
        var lastName = nameParts.Length > 1 ? nameParts[1] : firstName;
        return (firstName, lastName);
    }

    private static string GenerateTemporaryPassword()
    {
        var randomBytes = new byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        var randomPart = Convert.ToHexString(randomBytes);
        return $"Temp{randomPart}!123";
    }

    private static string TierToRoleString(EProviderTier tier) => tier switch
    {
        EProviderTier.Standard => UserRoles.ProviderStandard,
        EProviderTier.Silver => UserRoles.ProviderSilver,
        EProviderTier.Gold => UserRoles.ProviderGold,
        EProviderTier.Platinum => UserRoles.ProviderPlatinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
    };
}
