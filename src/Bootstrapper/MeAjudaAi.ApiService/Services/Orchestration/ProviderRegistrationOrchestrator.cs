using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Services.Orchestration;

public interface IProviderRegistrationOrchestrator
{
    Task<Result<ProviderDto>> RegisterProviderAsync(
        RegisterProviderRequest request,
        CancellationToken cancellationToken);
}

public sealed class ProviderRegistrationOrchestrator : IProviderRegistrationOrchestrator
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<ProviderRegistrationOrchestrator> _logger;

    public ProviderRegistrationOrchestrator(
        ICommandDispatcher commandDispatcher,
        ILogger<ProviderRegistrationOrchestrator> logger)
    {
        _commandDispatcher = commandDispatcher;
        _logger = logger;
    }

    public async Task<Result<ProviderDto>> RegisterProviderAsync(
        RegisterProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.AcceptedTerms || !request.AcceptedPrivacyPolicy)
            return Result<ProviderDto>.Failure(
                new Error("Você deve aceitar os Termos de Uso e a Política de Privacidade para se cadastrar.", 400));

        var phone = SanitizePhone(request.PhoneNumber);
        var username = GenerateUsername(phone);
        var (firstName, lastName) = SplitName(request.Name);
        var password = GenerateTemporaryPassword();

        var createUserResult = await CreateUserAsync(
            username, request.Email, firstName, lastName, password, request.PhoneNumber, cancellationToken);

        if (createUserResult.IsFailure)
        {
            _logger.LogError("Failed to create Keycloak user for provider registration. Error: {Error}",
                createUserResult.Error.Message);
            return Result<ProviderDto>.Failure(
                new Error("Ocorreu um erro ao registrar o usuário.", 400));
        }

        var userDto = createUserResult.Value;

        var providerResult = await CreateProviderAsync(
            userDto.Id, request.Name, request.Type, request.Email, request.PhoneNumber, cancellationToken);

        if (providerResult.IsFailure)
        {
            await CompensateUserCreationAsync(userDto.Id, cancellationToken);
            return Result<ProviderDto>.Failure(
                new Error("Ocorreu um erro ao registrar o provedor.", 400));
        }

        return Result<ProviderDto>.Success(providerResult.Value);
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

        return await _commandDispatcher.SendAsync<CreateUserCommand, Result<UserDto>>(
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

        return await _commandDispatcher.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
            createProviderCommand, cancellationToken);
    }

    private async Task CompensateUserCreationAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var deleteUserCommand = new DeleteUserCommand(userId);
            await _commandDispatcher.SendAsync<DeleteUserCommand, MeAjudaAi.Contracts.Functional.Result>(deleteUserCommand, cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Compensation cancelled while attempting to delete orphaned user {UserId}.", userId);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Compensation failed (timeout): Could not delete orphaned user {UserId}.", userId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Compensation failed (invalid operation): Could not delete orphaned user {UserId}.", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during compensation for orphaned user {UserId}.", userId);
        }
    }

    public static string SanitizePhone(string? phoneNumber)
    {
        return System.Text.RegularExpressions.Regex.Replace(phoneNumber ?? "", @"\D", "");
    }

    public static string GenerateUsername(string phone)
    {
        return string.IsNullOrEmpty(phone)
            ? $"provider_{Guid.NewGuid():N}"
            : $"provider_{phone}";
    }

    public static (string FirstName, string LastName) SplitName(string name)
    {
        var nameParts = name.Trim().Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : firstName;
        return (firstName, lastName);
    }

    public static string GenerateTemporaryPassword()
    {
        return $"Temp{Guid.NewGuid().ToString("N")[..8]}!123";
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
