using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Examples.OrdersModule.Application.Services;

/// <summary>
/// Exemplo de serviço em um módulo Orders que consome a API do módulo Users
/// </summary>
public class OrderValidationService
{
    private readonly IUsersModuleApi _usersApi;
    private readonly ILogger<OrderValidationService> _logger;

    public OrderValidationService(IUsersModuleApi usersApi, ILogger<OrderValidationService> logger)
    {
        _usersApi = usersApi;
        _logger = logger;
    }

    /// <summary>
    /// Valida se o usuário existe e pode criar um pedido
    /// </summary>
    public async Task<Result<bool>> ValidateUserCanCreateOrderAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating user {UserId} for order creation", userId);

        // 1. Verifica se o usuário existe usando a API do módulo Users
        var userExistsResult = await _usersApi.UserExistsAsync(userId, cancellationToken);
        
        if (userExistsResult.IsFailure)
        {
            _logger.LogError("Failed to check user existence: {Error}", userExistsResult.Error);
            return Result<bool>.Failure("Unable to validate user");
        }

        if (!userExistsResult.Value)
        {
            _logger.LogWarning("User {UserId} not found for order creation", userId);
            return Result<bool>.Failure("User not found");
        }

        // 2. Obtém dados do usuário para validações adicionais
        var userResult = await _usersApi.GetUserByIdAsync(userId, cancellationToken);
        
        if (userResult.IsFailure || userResult.Value == null)
        {
            _logger.LogError("Failed to get user details: {Error}", userResult.Error);
            return Result<bool>.Failure("Unable to get user details");
        }

        var user = userResult.Value;
        _logger.LogDebug("Found user: {Username} ({Email}) for order validation", user.Username, user.Email);

        // 3. Aqui poderiam vir outras validações específicas do módulo Orders
        // Por exemplo: verificar se o usuário tem conta ativa, não está bloqueado, etc.
        
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Exemplo de como buscar informações de múltiplos usuários em batch
    /// </summary>
    public async Task<Result<Dictionary<Guid, string>>> GetUserNamesForOrdersAsync(
        IReadOnlyList<Guid> userIds, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user names for {Count} orders", userIds.Count);

        var usersResult = await _usersApi.GetUsersBatchAsync(userIds, cancellationToken);
        
        if (usersResult.IsFailure)
        {
            _logger.LogError("Failed to get users batch: {Error}", usersResult.Error);
            return Result<Dictionary<Guid, string>>.Failure("Unable to get user information");
        }

        var userNames = usersResult.Value.ToDictionary(
            user => user.Id,
            user => $"{user.Username} ({user.Email})"
        );

        _logger.LogDebug("Retrieved names for {Count} users", userNames.Count);
        return Result<Dictionary<Guid, string>>.Success(userNames);
    }

    /// <summary>
    /// Exemplo de validação de email único para features específicas do módulo
    /// </summary>
    public async Task<Result<bool>> ValidateEmailForSpecialOrderAsync(
        string email, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating email {Email} for special order feature", email);

        var emailExistsResult = await _usersApi.EmailExistsAsync(email, cancellationToken);
        
        if (emailExistsResult.IsFailure)
        {
            return Result<bool>.Failure("Unable to validate email");
        }

        if (!emailExistsResult.Value)
        {
            return Result<bool>.Failure("Email not found in user system");
        }

        // Obtém dados do usuário pelo email
        var userResult = await _usersApi.GetUserByEmailAsync(email, cancellationToken);
        
        if (userResult.IsFailure || userResult.Value == null)
        {
            return Result<bool>.Failure("Unable to get user by email");
        }

        // Aqui poderiam vir validações específicas do módulo Orders
        // Por exemplo: verificar se é um usuário premium, etc.
        
        return Result<bool>.Success(true);
    }
}