using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.SearchProviders.API.Endpoints;

/// <summary>
/// Endpoint para buscar provedores de serviço por localização e critérios.
/// </summary>
public class SearchProvidersEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de busca de provedores.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = CreateVersionedGroup(app, "search", "Search");

        group.MapGet("/providers", SearchProvidersAsync)
            .WithName("SearchProviders")
            .WithSummary("Buscar prestadores de serviço")
            .WithDescription("""
                Busca prestadores de serviço ativos com base em geolocalização e filtros.
                
                **Algoritmo de Busca:**
                1. Filtrar por raio a partir da localização de busca
                2. Aplicar filtros opcionais (serviços, avaliação, nível de assinatura)
                3. Classificar resultados por:
                   - Nível de assinatura (Platinum > Gold > Standard > Free)
                   - Avaliação média (maior primeiro)
                   - Distância (mais próximo primeiro)
                
                **Casos de Uso:**
                - Encontrar prestadores próximos a uma localização específica
                - Buscar prestadores que oferecem serviços específicos
                - Filtrar por avaliação mínima ou nível de assinatura
                """)
            .Produces<PagedResult<SearchableProviderDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SearchProvidersAsync(
        IQueryDispatcher queryDispatcher,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusInKm,
        [FromQuery] Guid[]? serviceIds,
        [FromQuery] decimal? minRating,
        [FromQuery] ESubscriptionTier[]? subscriptionTiers,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Nota: Validação de entrada é tratada automaticamente por FluentValidation via pipeline do IQueryDispatcher
        // Veja SearchProvidersQueryValidator para as regras de validação
        var query = new SearchProvidersQuery(
            latitude,
            longitude,
            radiusInKm,
            serviceIds,
            minRating,
            subscriptionTiers,
            page,
            pageSize);

        var result = await queryDispatcher.QueryAsync<SearchProvidersQuery,
            Result<PagedResult<SearchableProviderDto>>>(
            query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new ProblemDetails
            {
                Title = "Busca Falhou",
                Detail = result.Error.Message,
                Status = StatusCodes.Status400BadRequest
            });
    }
}
