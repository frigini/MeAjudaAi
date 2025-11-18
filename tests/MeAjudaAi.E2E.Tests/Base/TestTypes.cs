namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Tipos reutilizáveis para testes E2E
/// </summary>
public static class TestTypes
{
    /// <summary>
    /// Representa uma resposta paginada genérica para testes
    /// </summary>
    public record PaginatedResponse<T>(
        IReadOnlyList<T> Data,
        int TotalCount,
        int PageNumber,
        int PageSize,
        bool HasNextPage,
        bool HasPreviousPage
    )
    {
        // Alias para compatibilidade com diferentes convenções de nomenclatura
        public IReadOnlyList<T> Items => Data;
        public int Page => PageNumber;
    }

    /// <summary>
    /// Representa uma resposta de token para testes de autenticação
    /// </summary>
    public record TokenResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType
    );
}
