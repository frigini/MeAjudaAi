using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;

/// <summary>
/// Local development implementation of IUserDomainService for environments where Keycloak is not available.
/// This service creates users locally without external authentication integration.
/// Used only for local development when Keycloak is disabled in configuration.
/// </summary>
internal class LocalDevelopmentUserDomainService : IUserDomainService
{
    /// <summary>
    /// Creates a user locally without Keycloak integration.
    /// Generates a mock Keycloak ID using UUID v7 for time-based ordering.
    /// </summary>
    public Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        // Para ambientes sem Keycloak, criar usuário mock com ID simulado
        // Using Guid.CreateVersion7() for better time-based ordering and performance
        var user = new User(username, email, firstName, lastName, $"mock_keycloak_{Guid.CreateVersion7()}");
        return Task.FromResult(Result<User>.Success(user));
    }

    /// <summary>
    /// Simulates synchronization with Keycloak.
    /// Always returns success for mock implementation.
    /// </summary>
    public Task<Result> SyncUserWithKeycloakAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Para ambientes sem Keycloak, simular sincronização bem-sucedida
        return Task.FromResult(Result.Success());
    }
}
