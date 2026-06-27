using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Modules.Ratings.API.Endpoints.Admin;
using MeAjudaAi.Modules.Ratings.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Ratings.
/// </summary>
[ExcludeFromCodeCoverage]
public static class RatingsEndpoints
{
    public const string Tag = "Ratings";

    /// <summary>
    /// Mapeia todos os endpoints do módulo Ratings.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Ratings.Base, "Ratings");

        group.MapEndpoint<CreateReviewEndpoint>();
        group.MapEndpoint<GetReviewByIdEndpoint>();
        group.MapEndpoint<GetProviderReviewsEndpoint>();
        group.MapEndpoint<GetReviewStatusEndpoint>();
    }
}
