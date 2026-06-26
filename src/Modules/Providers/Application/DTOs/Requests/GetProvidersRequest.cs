using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Requisição paginada para busca de prestadores com filtros opcionais.
/// </summary>
[ExcludeFromCodeCoverage]
public record GetProvidersRequest
{
    /// <summary>
    /// Filtro opcional por nome do prestador.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Filtro opcional por tipo de prestador.
    /// </summary>
    public int? Type { get; init; }

    /// <summary>
    /// Filtro opcional por status de verificação.
    /// </summary>
    public int? VerificationStatus { get; init; }

    /// <summary>
    /// Número da página para paginação.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Quantidade de itens por página.
    /// </summary>
    public int PageSize { get; init; } = 20;
}
