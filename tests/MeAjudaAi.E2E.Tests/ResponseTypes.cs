namespace MeAjudaAi.E2E.Tests;

public record CreateUserResponse(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string FullName,
    string KeycloakId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record UpdateUserResponse(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string FullName,
    string KeycloakId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record GetUserResponse(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string FullName,
    string KeycloakId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record PaginatedResponse<T>(
    IReadOnlyList<T> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    bool HasNextPage,
    bool HasPreviousPage
)
{
    // Alias para compatibilidade com os testes existentes
    public IReadOnlyList<T> Items => Data;
    public int Page => PageNumber;
}

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType
);
