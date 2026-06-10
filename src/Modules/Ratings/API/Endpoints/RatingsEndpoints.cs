using MeAjudaAi.Modules.Ratings.API.Endpoints.Admin;
using MeAjudaAi.Modules.Ratings.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints;

public static class RatingsEndpoints
{
    public const string Tag = "Ratings";

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Ratings.Base, "Ratings");

        group.MapEndpoint<CreateReviewEndpoint>();
        group.MapEndpoint<GetReviewByIdEndpoint>();
        group.MapEndpoint<GetProviderReviewsEndpoint>();
        group.MapEndpoint<GetReviewStatusEndpoint>();
    }
}

