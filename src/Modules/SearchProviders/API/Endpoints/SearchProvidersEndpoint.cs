using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;
using MeAjudaAi.Modules.SearchProviders.Application.Mappers;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Mvc;

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
        var group = CreateVersionedGroup(app, ApiEndpoints.SearchProviders.Base, "Search");

        group.MapGet("/providers", SearchProvidersAsync)
            .WithName("SearchProviders")
            .WithSummary("Buscar prestadores de serviço")
            .WithDescription("""
                Busca prestadores de serviço ativos com base em geolocalização e filtros.
                
                **Algoritmo de Busca:**
                1. Filtrar por raio a partir da localização de busca
                2. Aplicar filtro textual (nome, descrição) se fornecido
                3. Aplicar filtros opcionais (serviços, avaliação, nível de assinatura)
                4. Classificar resultados por:
                   - Nível de assinatura (Platinum > Gold > Standard > Free)
                   - Avaliação média (maior primeiro)
                   - Distância (mais próximo primeiro)
                
                **Casos de Uso:**
                - Encontrar prestadores próximos a uma localização específica
                - Buscar prestadores por nome ou termo (ex: "Eletricista", "João")
                - Buscar prestadores que oferecem serviços específicos
                - Filtrar por avaliação mínima ou nível de assinatura
                """)
            .Produces<PagedResult<ModuleSearchableProviderDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SearchProvidersAsync(
        IQueryDispatcher queryDispatcher,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusInKm,
        [FromQuery] string? term,
        [FromQuery] Guid[]? serviceIds,
        [FromQuery] decimal? minRating,
        [FromQuery] ESubscriptionTier[]? subscriptionTiers,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var domainTiers = subscriptionTiers?.Select(t => t.ToDomainTier()).ToArray();

        var query = new SearchProvidersQuery(
            latitude,
            longitude,
            radiusInKm,
            term,
            serviceIds,
            minRating,
            domainTiers,
            page,
            pageSize);

        var result = await queryDispatcher.QueryAsync<SearchProvidersQuery,
            Result<PagedResult<Application.DTOs.SearchableProviderDto>>>(
            query, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Busca Falhou",
                Detail = result.Error.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }

        var pagedResult = result.Value!;
        var mappedItems = pagedResult.Items.Select(MapToModuleDto).ToList();

        var response = new PagedResult<ModuleSearchableProviderDto>
        {
            Items = mappedItems,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalItems
        };

        return Results.Ok(response);
    }

    private static ModuleSearchableProviderDto MapToModuleDto(Application.DTOs.SearchableProviderDto dto) => new(
        ProviderId: dto.ProviderId,
        Name: dto.Name,
        Slug: dto.Slug,
        Location: new ModuleLocationDto(
            Latitude: dto.Location.Latitude,
            Longitude: dto.Location.Longitude),
        AverageRating: dto.AverageRating,
        TotalReviews: dto.TotalReviews,
        SubscriptionTier: dto.SubscriptionTier.ToModuleTier(),
        ServiceIds: dto.ServiceIds,
        Description: dto.Description,
        DistanceInKm: dto.DistanceInKm,
        City: dto.City,
        State: dto.State);
}
